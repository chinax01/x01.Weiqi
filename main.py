# main.py
import asyncio
import logging
from typing import Dict, List, Optional, Tuple
from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from fastapi.staticfiles import StaticFiles
from fastapi.responses import HTMLResponse

app = FastAPI()
app.mount("/static", StaticFiles(directory="static"), name="static")

# ------------------- 围棋规则引擎 -------------------
class GameState:
    """19路围棋状态管理（无AI，纯双人对弈）"""
    def __init__(self, board_size: int = 19):
        self.board_size = board_size
        self.board = [[0] * board_size for _ in range(board_size)]  # 0空 1黑 2白
        self.black_turn = True          # True=黑棋走
        self.black_captures = 0
        self.white_captures = 0
        self.ko_point: Optional[Tuple[int, int]] = None   # 劫争禁止点 (row, col)
        self.game_over = False
        self.winner: Optional[int] = None                  # 1黑胜 2白胜
        self.consecutive_passes = 0                        # 连续虚着次数
        self._prev_board_hash = ""                         # 上一步棋盘哈希，用于劫争检测

    def _board_hash(self) -> str:
        """棋盘状态快速哈希（用于全局同型检测）"""
        return ''.join(str(cell) for row in self.board for cell in row)

    def _get_group(self, row: int, col: int, visited: set) -> List[Tuple[int, int]]:
        """获取相连的相同颜色棋子组"""
        color = self.board[row][col]
        if color == 0:
            return []
        group = []
        stack = [(row, col)]
        while stack:
            r, c = stack.pop()
            if (r, c) in visited:
                continue
            visited.add((r, c))
            if self.board[r][c] != color:
                continue
            group.append((r, c))
            for dr, dc in [(1,0), (-1,0), (0,1), (0,-1)]:
                nr, nc = r + dr, c + dc
                if 0 <= nr < self.board_size and 0 <= nc < self.board_size:
                    if (nr, nc) not in visited and self.board[nr][nc] == color:
                        stack.append((nr, nc))
        return group

    def _count_liberties(self, group: List[Tuple[int, int]]) -> int:
        """计算一组棋子的气数"""
        liberties = set()
        for r, c in group:
            for dr, dc in [(1,0), (-1,0), (0,1), (0,-1)]:
                nr, nc = r + dr, c + dc
                if 0 <= nr < self.board_size and 0 <= nc < self.board_size:
                    if self.board[nr][nc] == 0:
                        liberties.add((nr, nc))
        return len(liberties)

    def _remove_group(self, group: List[Tuple[int, int]]):
        """移除一组棋子并增加提子计数"""
        for r, c in group:
            color = self.board[r][c]
            if color == 1:
                self.black_captures += 1
            elif color == 2:
                self.white_captures += 1
            self.board[r][c] = 0

    def _is_suicide(self, row: int, col: int, color: int) -> bool:
        """检查落子后己方棋子是否自杀（无气）"""
        # 临时放置棋子
        self.board[row][col] = color
        # 获取包含该棋子的整个组
        visited = set()
        group = self._get_group(row, col, visited)
        liberties = self._count_liberties(group)
        self.board[row][col] = 0  # 恢复
        return liberties == 0

    def place_stone(self, row: int, col: int) -> bool:
        """落子主逻辑，返回是否合法"""
        if self.game_over:
            return False
        if not (0 <= row < self.board_size and 0 <= col < self.board_size):
            return False
        if self.board[row][col] != 0:
            return False

        # 劫争禁止
        if self.ko_point == (row, col):
            return False

        color = 1 if self.black_turn else 2
        # 保存当前状态用于回滚
        old_board = [r[:] for r in self.board]
        old_captures = (self.black_captures, self.white_captures)
        old_ko = self.ko_point
        old_hash = self._board_hash()

        # 尝试落子
        self.board[row][col] = color
        # 检查并移除相邻的对方无气棋子
        opponent_color = 3 - color
        groups_to_remove = []
        for dr, dc in [(1,0), (-1,0), (0,1), (0,-1)]:
            nr, nc = row + dr, col + dc
            if 0 <= nr < self.board_size and 0 <= nc < self.board_size:
                if self.board[nr][nc] == opponent_color:
                    visited = set()
                    group = self._get_group(nr, nc, visited)
                    if self._count_liberties(group) == 0:
                        groups_to_remove.append(group)
        # 执行提子
        for group in groups_to_remove:
            self._remove_group(group)

        # 自杀检测（落子后自己的组必须还有气）
        visited_self = set()
        self_group = self._get_group(row, col, visited_self)
        if self._count_liberties(self_group) == 0:
            # 自杀，回滚
            self.board = old_board
            self.black_captures, self.white_captures = old_captures
            self.ko_point = old_ko
            return False

        # 全局同型检测（劫争进阶保护）
        new_hash = self._board_hash()
        if new_hash == self._prev_board_hash and self._prev_board_hash != "":
            self.board = old_board
            self.black_captures, self.white_captures = old_captures
            self.ko_point = old_ko
            return False

        # 更新劫点：如果本次提子恰好一子，且被提的棋子在刚落子位置，则设置劫点
        self.ko_point = None
        if len(groups_to_remove) == 1 and len(groups_to_remove[0]) == 1:
            removed_row, removed_col = groups_to_remove[0][0]
            # 劫点是被提子的位置
            self.ko_point = (removed_row, removed_col)

        # 记录上一步棋盘哈希
        self._prev_board_hash = old_hash
        # 切换回合
        self.black_turn = not self.black_turn
        self.consecutive_passes = 0   # 重置连pass计数
        return True

    def pass_move(self) -> bool:
        """虚着"""
        if self.game_over:
            return False
        self.consecutive_passes += 1
        self.black_turn = not self.black_turn
        self.ko_point = None   # 虚着清除劫点
        self._prev_board_hash = self._board_hash()  # 记录状态用于同型检测

        if self.consecutive_passes >= 2:
            # 双方连续pass，对局自然结束（胜负未定，仅标记结束）
            self.game_over = True
            self.winner = None   # 由前端或外部协商，或通过数子决定（简化处理）
        return True

    def resign(self, player_color: int) -> bool:
        """认输"""
        if self.game_over:
            return False
        self.game_over = True
        self.winner = 3 - player_color  # 对方获胜
        return True

    def save_game_state(self):
        """为兼容原接口，保存当前状态（无需额外实现）"""
        pass

# ------------------- 状态转换工具 -------------------
def game_state_to_frontend(game: GameState) -> dict:
    """将GameState转换为前端所需JSON格式"""
    size = game.board_size
    # 行列转置：前端 board[x][y] 对应 x列 y行
    board_transposed = [[0] * size for _ in range(size)]
    for r in range(size):
        for c in range(size):
            board_transposed[c][r] = game.board[r][c]

    turn = 1 if game.black_turn else 2
    ko = None
    if game.ko_point:
        ko = [game.ko_point[1], game.ko_point[0]]   # 转置为 [x,y]

    return {
        "board": board_transposed,
        "turn": turn,
        "game_over": game.game_over,
        "winner": game.winner,
        "ko": ko,
        "captures": [game.black_captures, game.white_captures],  # 黑, 白
    }

# ------------------- 房间管理（纯双人对弈）-------------------
class ConnectionManager:
    def __init__(self):
        self.rooms: Dict[str, Dict[str, WebSocket]] = {}   # room_id -> {"1": ws, "2": ws}
        self.games: Dict[str, GameState] = {}              # room_id -> GameState

    async def join_room(self, room_id: str, websocket: WebSocket) -> bool:
        """玩家加入房间，房间满2人开始游戏"""
        if room_id not in self.rooms:
            self.rooms[room_id] = {}
            self.games[room_id] = GameState(board_size=19)

        if len(self.rooms[room_id]) >= 2:
            await websocket.accept()
            await websocket.send_json({"type": "error", "message": "房间已满"})
            await websocket.close()
            return False

        await websocket.accept()
        color = 1 if len(self.rooms[room_id]) == 0 else 2
        self.rooms[room_id][str(color)] = websocket
        await websocket.send_json({"type": "assign_color", "color": color})

        # 两人到齐，游戏开始
        if len(self.rooms[room_id]) == 2:
            game = self.games[room_id]
            await self.broadcast(room_id, {"type": "game_start", "state": game_state_to_frontend(game)})
        else:
            await websocket.send_json({"type": "waiting", "message": "等待对手加入..."})
        return True

    def leave_room(self, room_id: str, color: str):
        if room_id in self.rooms:
            if color in self.rooms[room_id]:
                del self.rooms[room_id][color]
            if not self.rooms[room_id]:
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
        # 发送初始状态
        await websocket.send_json({"type": "state_update", "state": game_state_to_frontend(game)})

        while True:
            msg = await websocket.receive_json()
            if msg["type"] == "move":
                x, y = msg["x"], msg["y"]
                player_color = msg["color"]
                current_turn = 1 if game.black_turn else 2
                if game.game_over:
                    await websocket.send_json({"type": "error", "message": "游戏已结束"})
                    continue
                if player_color != current_turn:
                    await websocket.send_json({"type": "error", "message": "还没轮到你"})
                    continue
                # 前端传 (x列, y行) -> 转换为 (row, col)
                success = game.place_stone(y, x)
                if not success:
                    await websocket.send_json({"type": "error", "message": "无效落子"})
                    continue
                # 广播新状态
                await manager.broadcast(room_id, {"type": "state_update", "state": game_state_to_frontend(game)})

            elif msg["type"] == "pass":
                player_color = msg["color"]
                current_turn = 1 if game.black_turn else 2
                if game.game_over:
                    await websocket.send_json({"type": "error", "message": "游戏已结束"})
                    continue
                if player_color != current_turn:
                    await websocket.send_json({"type": "error", "message": "还没轮到你"})
                    continue
                game.pass_move()
                await manager.broadcast(room_id, {"type": "state_update", "state": game_state_to_frontend(game)})
                if game.game_over:
                    await manager.broadcast(room_id, {"type": "game_over", "message": "双方连续虚着，对局结束"})

            elif msg["type"] == "resign":
                player_color = msg["color"]
                if game.game_over:
                    await websocket.send_json({"type": "error", "message": "游戏已结束"})
                    continue
                game.resign(player_color)
                winner_name = "黑方" if game.winner == 1 else "白方"
                await manager.broadcast(room_id, {"type": "game_over", "message": f"{winner_name} 认输，对方获胜！"})
                await manager.broadcast(room_id, {"type": "state_update", "state": game_state_to_frontend(game)})

    except WebSocketDisconnect:
        if room_id in manager.rooms and color_str:
            manager.leave_room(room_id, color_str)
            await manager.broadcast(room_id, {"type": "opponent_left", "message": "对手断开连接"})

if __name__ == '__main__':
    import uvicorn
    uvicorn.run('main:app', reload=False, host='0.0.0.0', port=8000)