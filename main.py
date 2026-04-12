import asyncio
import logging
from typing import Dict
from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from fastapi.staticfiles import StaticFiles
from fastapi.responses import HTMLResponse

# 导入我们的游戏状态和 AI 模块
from state import GameState

app = FastAPI()
app.mount("/static", StaticFiles(directory="static"), name="static")

# ------------------- 状态转换工具 -------------------
def game_state_to_frontend(game: GameState) -> dict:
    """
    将 GameState 对象转换为前端期望的格式
    {
        "board": 二维列表 (0空 1黑 2白),
        "turn": 1 (黑) 或 2 (白),
        "game_over": bool,
        "winner": None 或 1/2,
        "ko": [x, y] 或 None
    }
    """
    # 棋盘转换：GameState.board 是行主序，但前端绘制使用 board[x][y] 其中 x 是列，y 是行？
    # 查看前端 drawBoard 逻辑：for i (x) for j (y) board[i][j]，所以需要行列对应。
    # GameState.board 是 board[row][col]，需转置为 board[col][row] 以匹配前端。
    board_rows = game.board
    size = game.board_size
    board_transposed = [[0] * size for _ in range(size)]
    for r in range(size):
        for c in range(size):
            board_transposed[c][r] = board_rows[r][c]

    turn = 1 if game.black_turn else 2
    game_over = False  # GameState 没有 game_over 标志，可自行扩展，这里暂用 False
    winner = None      # 同样，可后续扩展计分判断
    ko = None
    if game.ko_point and isinstance(game.ko_point, dict):
        # ko_point 存储为 {'row': r, 'col': c}，前端期望 [x, y] 对应列、行
        ko = [game.ko_point['col'], game.ko_point['row']]

    return {
        "board": board_transposed,
        "turn": turn,
        "game_over": game_over,
        "winner": winner,
        "ko": ko,
    }


# ------------------- 房间管理 -------------------
class ConnectionManager:
    def __init__(self):
        self.rooms: Dict[str, Dict[str, WebSocket]] = {}   # room_id -> {color: websocket}
        self.games: Dict[str, GameState] = {}              # room_id -> GameState
        self.ai_task: Dict[str, asyncio.Task] = {}         # AI 思考后台任务

    async def join_room(self, room_id: str, websocket: WebSocket):
        """玩家加入房间，AI 房间前缀 'ai:' 表示人机对战"""
        is_ai_room = room_id.startswith("ai:")
        if room_id not in self.rooms:
            self.rooms[room_id] = {}
            game = GameState(board_size=19)
            self.games[room_id] = game
            if is_ai_room:
                # 启用 AI，默认 AI 执白 (2)，难度 hard
                game.enable_ai(difficulty='hard', ai_color=2)
                logging.info(f"AI room {room_id} created, AI plays white.")
        # 最大玩家数：AI 房间 1 人，普通房间 2 人
        max_players = 1 if is_ai_room else 2
        if len(self.rooms[room_id]) >= max_players:
            await websocket.accept()
            await websocket.send_json({"type": "error", "message": "房间已满"})
            await websocket.close()
            return False

        await websocket.accept()
        color = 1 if len(self.rooms[room_id]) == 0 else 2
        self.rooms[room_id][str(color)] = websocket
        await websocket.send_json({"type": "assign_color", "color": color})

        # 检查是否达到开始条件
        game = self.games[room_id]
        can_start = False
        if is_ai_room and len(self.rooms[room_id]) == 1:
            can_start = True
        elif not is_ai_room and len(self.rooms[room_id]) == 2:
            can_start = True

        if can_start:
            await self.broadcast(room_id, {"type": "game_start", "state": game_state_to_frontend(game)})
            # 如果 AI 房间且 AI 执黑先手，则立即触发 AI 第一步
            if is_ai_room:
                human_color = color
                ai_color = 3 - human_color
                if game.black_turn and ai_color == 1:  # AI 执黑且当前轮到黑
                    self._schedule_ai_move(room_id)
        else:
            await websocket.send_json({"type": "waiting", "message": "等待对手加入..."})
        return True

    def leave_room(self, room_id: str, color: str):
        if room_id in self.rooms:
            if color in self.rooms[room_id]:
                del self.rooms[room_id][color]
            if not self.rooms[room_id]:
                # 清理资源
                if room_id in self.ai_task:
                    self.ai_task[room_id].cancel()
                    del self.ai_task[room_id]
                del self.rooms[room_id]
                if room_id in self.games:
                    del self.games[room_id]

    async def broadcast(self, room_id: str, message: dict):
        if room_id in self.rooms:
            for ws in self.rooms[room_id].values():
                try:
                    await ws.send_json(message)
                except Exception:
                    pass

    def _schedule_ai_move(self, room_id: str):
        """调度 AI 走子（非阻塞）"""
        if room_id in self.ai_task:
            self.ai_task[room_id].cancel()
        task = asyncio.create_task(self._ai_move_task(room_id))
        self.ai_task[room_id] = task

    async def _ai_move_task(self, room_id: str):
        """后台任务：稍作延迟后调用 AI 走一步"""
        await asyncio.sleep(0.5)  # 模拟思考
        game = self.games.get(room_id)
        if not game:
            return
        if not room_id.startswith("ai:"):
            return
        # 检查是否轮到 AI
        human_color = None
        for c in self.rooms[room_id].keys():
            human_color = int(c)
            break
        ai_color = 3 - human_color
        current_turn = 1 if game.black_turn else 2
        if current_turn != ai_color:
            return

        # AI 走子
        success = game.make_ai_move()
        # 广播新状态
        await self.broadcast(room_id, {"type": "state_update", "state": game_state_to_frontend(game)})
        # 如果游戏没有结束且仍然轮到 AI（比如连续 pass 后），继续调度
        # 注意：make_ai_move 内部会切换回合，通常不会连续 AI，但为了安全检查
        if game.black_turn == (ai_color == 1):
            self._schedule_ai_move(room_id)


manager = ConnectionManager()


@app.get("/")
async def get():
    return HTMLResponse(content=open("static/index.html", encoding="utf-8").read())


@app.websocket("/ws/{room_id}")
async def websocket_endpoint(websocket: WebSocket, room_id: str):
    color_str = None
    try:
        joined = await manager.join_room(room_id, websocket)
        if not joined:
            return
        # 等待客户端确认颜色
        data = await websocket.receive_json()
        if data.get("type") == "color_ack":
            color_str = str(data["color"])
        else:
            return

        game = manager.games[room_id]

        # 发送当前状态
        await websocket.send_json({"type": "state_update", "state": game_state_to_frontend(game)})

        while True:
            msg = await websocket.receive_json()
            if msg["type"] == "move":
                x, y = msg["x"], msg["y"]
                player_color = msg["color"]
                # 检查游戏是否结束（可根据需要扩展）
                # 检查是否轮到玩家
                current_turn = 1 if game.black_turn else 2
                if player_color != current_turn:
                    await websocket.send_json({"type": "error", "message": "还没轮到你"})
                    continue
                # 调用 GameState 落子（注意行列转换：前端 board[x][y] 中 x 是列，y 是行）
                # place_stone 参数是 (row, col)，所以传入 (y, x)
                success = game.place_stone(y, x)
                if not success:
                    await websocket.send_json({"type": "error", "message": "无效落子"})
                    continue
                # 广播新状态
                await manager.broadcast(room_id, {"type": "state_update", "state": game_state_to_frontend(game)})

                # 如果是 AI 房间，落子后可能轮到 AI
                if room_id.startswith("ai:"):
                    manager._schedule_ai_move(room_id)

            elif msg["type"] == "pass":
                player_color = msg["color"]
                current_turn = 1 if game.black_turn else 2
                if player_color != current_turn:
                    await websocket.send_json({"type": "error", "message": "还没轮到你"})
                    continue
                # 虚着：直接切换回合
                game.black_turn = not game.black_turn
                game.save_game_state()
                await manager.broadcast(room_id, {"type": "state_update", "state": game_state_to_frontend(game)})
                # 检查是否连续 pass 导致终局（可扩展）
                if room_id.startswith("ai:"):
                    manager._schedule_ai_move(room_id)

            elif msg["type"] == "resign":
                player_color = msg["color"]
                winner = "白" if player_color == 1 else "黑"
                # 标记游戏结束（这里可以设置一个 game_over 标志，但 GameState 没有，直接广播消息）
                await manager.broadcast(room_id, {"type": "game_over", "message": f"对手认输，{winner}胜！"})

    except WebSocketDisconnect:
        if room_id in manager.rooms and color_str:
            manager.leave_room(room_id, color_str)
            await manager.broadcast(room_id, {"type": "opponent_left", "message": "对手断开连接"})


if __name__ == '__main__':
    import uvicorn
    uvicorn.run('main:app', reload=True, host="0.0.0.0", port=8000)