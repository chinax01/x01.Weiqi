"""
围棋游戏核心逻辑 (Python版本)
"""
from goai import GoAI
from katrain.core.game_node import GameNode
from katrain.core.sgf_parser import Move  # MODIFIED: 导入Move类用于构建游戏树节点

class GameState:
    def __init__(self, board_size=19):
        self.board_size = board_size
        self.black_turn = True
        self.board = []
        self.move_numbers = []
        self.last_move = None
        self.ko_point = None
        self.black_captured = 0
        self.white_captured = 0
        self.current_move_number = 0
        
        # 地盘计算相关
        self.territory_board = None
        self.show_territory = False
        
        # 悔棋相关
        self.move_history = []
        self.board_history = []
        self.move_numbers_history = []
        self.ko_history = []
        self.captured_history = []
        self.max_undo_steps = 350
        
        # 显示设置
        self.show_move_numbers = False
        self.move_number_font_size = 12

        # AI 相关属性
        self.ai_enabled = False
        self.ai_difficulty = 'hard'
        self.ai_playing = False
        self.ai_color = 2  # 默认AI执白
        self.ai_thinking_time = 1.0  # AI思考时间（秒）
        self.goai = GoAI(self, self.ai_difficulty)
        
        self.root_node = GameNode(parent=None)  # 根节点无落子
        self.current_node = self.root_node
        self.move_tree = self.root_node  # 树根
        
        self.initialize_board()
    
    # 添加 AI 相关方法
    def enable_ai(self, difficulty='hard', ai_color=2):
        """启用AI"""
        self.ai_enabled = True
        self.ai_difficulty = difficulty
        self.ai_color = ai_color
        self.ai_playing = True

    def disable_ai(self):
        """禁用AI"""
        self.ai_enabled = False
        self.ai_playing = False

    def is_ai_turn(self):
        """检查是否是AI回合"""
        if not self.ai_enabled or not self.ai_playing:
            return False
        
        current_stone = 1 if self.black_turn else 2
        return current_stone == self.ai_color

    def get_ai_move(self):
        """获取AI落子位置"""
        if not self.is_ai_turn():
            return None
        
        # 导入AI模块
        try:
            self.goai.difficulty = self.ai_difficulty
            return self.goai.get_best_move()
        except ImportError as e:
            print(f"AI模块导入失败: {e}")
            return None

    def make_ai_move(self):
        """AI落子"""
        move = self.get_ai_move()
        if not move:
            print("AI无法找到合法落子位置，跳过回合")
            # 切换回合，让对手继续
            self.black_turn = not self.black_turn
            return False
        
        row, col = move
        print(f"AI尝试落子: ({row+1},{col+1})")
        
        # 尝试落子
        if self.place_stone(row, col):
            return True
        
        print("AI落子失败，尝试其他位置...")
        try:
            # 获取当前AI实例（如果有的话）
            if hasattr(self, '_ai_instance'):
                ai = self._ai_instance
                if hasattr(ai, 'invalid_positions'):
                    ai.invalid_positions.add((row, col))
        except:  # noqa: E722
            pass
        
        # 获取所有合法落子位置
        stone = 1 if self.black_turn else 2
        legal_moves = []
        
        for r in range(self.board_size):
            for c in range(self.board_size):
                if self.is_valid_move(r, c, stone):
                    legal_moves.append((r, c))
        
        # 移除已经尝试过的位置
        if (row, col) in legal_moves:
            legal_moves.remove((row, col))
        
        # 尝试其他合法位置（最多尝试5个）
        max_tries = min(5, len(legal_moves))
        for i in range(max_tries):
            r, c = legal_moves[i]
            print(f"AI尝试备用位置 {i+1}/{max_tries}: ({r+1},{c+1})")
            if self.place_stone(r, c):
                return True
        
        print("AI无法找到合法落子位置，跳过回合")
        # 切换回合，让对手继续
        self.black_turn = not self.black_turn
        return False

    def initialize_board(self):
        """初始化棋盘"""
        self.board = []
        self.move_numbers = []
        self.territory_board = []
        
        for i in range(self.board_size):
            self.board.append([0] * self.board_size)  # 0: 空, 1: 黑棋, 2: 白棋
            self.move_numbers.append([0] * self.board_size)  # 0: 没有步数
            self.territory_board.append([0] * self.board_size)  # 0: 未标记地盘
        
        self.last_move = None
        self.ko_point = None
        self.black_captured = 0
        self.white_captured = 0
        self.current_move_number = 0
        self.show_territory = False
        
        # 清空历史记录
        self.move_history = []
        self.board_history = []
        self.move_numbers_history = []
        self.ko_history = []
        self.captured_history = []
        
        # MODIFIED: 重置游戏树
        self.root_node = GameNode(parent=None)
        self.current_node = self.root_node
        self.move_tree = self.root_node
        
        # 保存初始状态
        self.save_game_state()
    
    def save_game_state(self):
        """保存游戏状态"""
        # 保存棋盘状态
        board_state = []
        move_numbers_state = []
        for i in range(self.board_size):
            board_state.append(self.board[i].copy())
            move_numbers_state.append(self.move_numbers[i].copy())
        
        self.board_history.append(board_state)
        self.move_numbers_history.append(move_numbers_state)
        
        # 保存劫点状态 - 确保不是 None 对象
        ko_state = None
        if self.ko_point and isinstance(self.ko_point, dict):
            ko_state = {'row': self.ko_point.get('row', -1), 'col': self.ko_point.get('col', -1)}
        self.ko_history.append(ko_state)
        
        # 保存提子计数和步数
        captured_state = {
            'black': self.black_captured,
            'white': self.white_captured,
            'move_number': self.current_move_number
        }
        self.captured_history.append(captured_state)
        
        # 保存落子信息 - 确保不是 None 对象
        move_state = None
        if self.last_move and isinstance(self.last_move, dict):
            move_state = {
                'row': self.last_move.get('row', -1),
                'col': self.last_move.get('col', -1),
                'stone': self.last_move.get('stone', 0)
            }
        self.move_history.append(move_state)
        
        # 限制历史记录长度
        if len(self.board_history) > self.max_undo_steps:
            self.board_history.pop(0)
            self.move_numbers_history.pop(0)
            self.ko_history.pop(0)
            self.captured_history.pop(0)
            self.move_history.pop(0)
        
        return len(self.board_history)
    
    # 修复 undo_move 方法
    def undo_move(self):
        """悔棋"""
        if len(self.board_history) <= 1:
            print("无法悔棋：已经是初始状态")
            return False
        
        # MODIFIED: 回退游戏树当前节点到父节点
        if self.current_node.parent is not None:
            self.current_node = self.current_node.parent
        # 注意：根节点无父节点，但 len>1 时 current_node 不可能是根节点，所以安全
        
        # 移除当前状态
        self.board_history.pop()
        self.move_numbers_history.pop()
        self.ko_history.pop()
        self.captured_history.pop()
        self.move_history.pop()  # 这里会同时移除最后一步的落子信息
        
        # 恢复到上一个状态
        prev_board_state = self.board_history[-1]
        prev_move_numbers_state = self.move_numbers_history[-1]
        prev_ko_state = self.ko_history[-1]
        prev_captured_state = self.captured_history[-1]
        prev_move = self.move_history[-1]
        
        # 恢复棋盘状态
        for i in range(self.board_size):
            for j in range(self.board_size):
                self.board[i][j] = prev_board_state[i][j]
                self.move_numbers[i][j] = prev_move_numbers_state[i][j]
        
        # 恢复劫点状态 - 安全处理 None
        if prev_ko_state and isinstance(prev_ko_state, dict):
            self.ko_point = {'row': prev_ko_state.get('row', -1), 'col': prev_ko_state.get('col', -1)}
        else:
            self.ko_point = None
        
        # 恢复提子计数和步数
        self.black_captured = prev_captured_state.get('black', 0)
        self.white_captured = prev_captured_state.get('white', 0)
        self.current_move_number = prev_captured_state.get('move_number', 0)
        
        # 恢复最后落子信息 - 安全处理 None
        if prev_move and isinstance(prev_move, dict):
            self.last_move = {
                'row': prev_move.get('row', -1),
                'col': prev_move.get('col', -1),
                'stone': prev_move.get('stone', 0)
            }
            # 切换回合到上一步的对手
            self.black_turn = (prev_move.get('stone', 1) == 2)
        else:
            self.last_move = None
            self.black_turn = True
        
        # 清除地盘显示
        self.clear_territory()
        
        return True
    
    def clear_territory(self):
        """清除地盘显示"""
        for i in range(self.board_size):
            for j in range(self.board_size):
                self.territory_board[i][j] = 0
        self.show_territory = False
    
    def is_valid_position(self, row, col):
        """检查位置是否有效"""
        return 0 <= row < self.board_size and 0 <= col < self.board_size
    
    def is_valid_move(self, row, col, stone):
        """检查落子是否有效"""
        if not self.is_valid_position(row, col) or self.board[row][col] != 0:
            return False
        
        # 检查劫点禁止
        if self.ko_point and row == self.ko_point['row'] and col == self.ko_point['col']:
            return False  # 直接禁止在劫点落子
        
        # 尝试落子
        temp_stone = self.board[row][col]
        self.board[row][col] = stone
        
        # 提子检查
        captured_stones = self.get_captured_stones(row, col, stone)
        
        # 检查自杀
        has_liberties = self.count_liberties(row, col) > 0
        
        # 恢复原状
        self.board[row][col] = temp_stone
        for captured in captured_stones:
            self.board[captured['row']][captured['col']] = (2 if stone == 1 else 1)
        
        if not has_liberties and len(captured_stones) == 0:
            return False  # 自杀
        
        return True
    
    def get_captured_stones(self, row, col, stone):
        """获取可以提掉的棋子"""
        captured = []
        opponent = 2 if stone == 1 else 1
        
        # 使用集合来跟踪已经检查过的位置，避免重复
        visited_positions = set()
        
        # 检查四个方向的对手棋子
        directions = [(-1, 0), (1, 0), (0, -1), (0, 1)]
        for dr, dc in directions:
            nr, nc = row + dr, col + dc
            
            if self.is_valid_position(nr, nc) and self.board[nr][nc] == opponent:
                # 检查这个位置是否已经处理过
                position_key = (nr, nc)
                if position_key in visited_positions:
                    continue
                    
                group = self.get_stone_group(nr, nc)
                liberties = self.count_group_liberties(nr, nc)
                
                if liberties == 0:
                    # 标记这个组中的所有位置为已处理
                    for stone_pos in group:
                        pos_key = (stone_pos['row'], stone_pos['col'])
                        visited_positions.add(pos_key)
                    
                    captured.extend(group)
        
        return captured
    
    def get_stone_group(self, row, col):
        """获取棋子所在的气块"""
        stone = self.board[row][col]
        group = []
        stack = [{'row': row, 'col': col}]
        visited = [[False] * self.board_size for _ in range(self.board_size)]
        
        while stack:
            current = stack.pop()
            if visited[current['row']][current['col']]:
                continue
            
            visited[current['row']][current['col']] = True
            group.append(current)
            
            directions = [(-1, 0), (1, 0), (0, -1), (0, 1)]
            for dr, dc in directions:
                nr, nc = current['row'] + dr, current['col'] + dc
                if (self.is_valid_position(nr, nc) and 
                    self.board[nr][nc] == stone and 
                    not visited[nr][nc]):
                    stack.append({'row': nr, 'col': nc})
        
        return group
    
    def count_group_liberties(self, row, col):
        """计算气块的气数"""
        stone = self.board[row][col]
        liberties = 0
        visited = [[False] * self.board_size for _ in range(self.board_size)]
        liberty_visited = [[False] * self.board_size for _ in range(self.board_size)]
        
        stack = [{'row': row, 'col': col}]
        while stack:
            current = stack.pop()
            if visited[current['row']][current['col']]:
                continue
            
            visited[current['row']][current['col']] = True
            
            directions = [(-1, 0), (1, 0), (0, -1), (0, 1)]
            for dr, dc in directions:
                nr, nc = current['row'] + dr, current['col'] + dc
                
                if self.is_valid_position(nr, nc):
                    if self.board[nr][nc] == 0 and not liberty_visited[nr][nc]:
                        liberties += 1
                        liberty_visited[nr][nc] = True
                    elif self.board[nr][nc] == stone and not visited[nr][nc]:
                        stack.append({'row': nr, 'col': nc})
        
        return liberties
    
    def count_liberties(self, row, col):
        """计算棋子的气"""
        stone = self.board[row][col]
        if stone == 0:
            return 0
        
        liberties = 0
        directions = [(-1, 0), (1, 0), (0, -1), (0, 1)]
        
        for dr, dc in directions:
            nr, nc = row + dr, col + dc
            if self.is_valid_position(nr, nc) and self.board[nr][nc] == 0:
                liberties += 1
        
        # 还需要检查与同色棋子连接后的气
        for dr, dc in directions:
            nr, nc = row + dr, col + dc
            if (self.is_valid_position(nr, nc) and 
                self.board[nr][nc] == stone):
                liberties = max(liberties, self.count_group_liberties(nr, nc))
        
        return liberties
    
    def check_ko(self, row, col, stone, captured_stones):
        """检查是否形成劫争"""
        # 只有提了一个子才可能是劫
        if len(captured_stones) != 1:
            return None
        
        captured_stone = captured_stones[0]
        opponent = 2 if stone == 1 else 1
        
        # 临时移除被提的棋子
        temp = self.board[captured_stone['row']][captured_stone['col']]
        self.board[captured_stone['row']][captured_stone['col']] = 0
        
        # 检查对方是否可以立即反提
        can_recapture = False
        
        # 检查被提位置是否可以下子
        if self.is_valid_position(captured_stone['row'], captured_stone['col']):
            # 模拟在被提位置下子
            self.board[captured_stone['row']][captured_stone['col']] = opponent
            recapture_stones = self.get_captured_stones(
                captured_stone['row'], captured_stone['col'], opponent
            )
            
            # 如果反提也只提了一个子，且位置相同，则形成劫
            if (len(recapture_stones) == 1 and 
                recapture_stones[0]['row'] == row and 
                recapture_stones[0]['col'] == col):
                can_recapture = True
            
            self.board[captured_stone['row']][captured_stone['col']] = 0
        
        # 恢复被提的棋子
        self.board[captured_stone['row']][captured_stone['col']] = temp
        
        if can_recapture:
            return {
                'row': captured_stone['row'],
                'col': captured_stone['col']
            }
        
        return None
    
    def place_stone(self, row, col):
        """放置棋子"""
        stone = 1 if self.black_turn else 2
        
        print(f"=== 尝试落子: ({row+1},{col+1}), 颜色: {'黑' if stone == 1 else '白'} ===")
        
        # 检查是否有效落子
        if not self.is_valid_move(row, col, stone):
            print("无效落子")
            return False
        
        # 增加步数
        self.current_move_number += 1
        
        # 放置棋子并记录步数
        self.board[row][col] = stone
        self.move_numbers[row][col] = self.current_move_number
        
        self.last_move = {
            'row': row,
            'col': col,
            'stone': stone
        }
        
        player = 'B' if stone == 1 else 'W'
        move_obj = Move((col, row), player=player)  # SGF坐标: (列, 行)
        new_node = GameNode(parent=self.current_node, move=move_obj)
        self.current_node = new_node
        
        # 提子
        captured_stones = self.get_captured_stones(row, col, stone)
        print(f"找到提子数: {len(captured_stones)}")
        
        if captured_stones:
            for captured in captured_stones:
                print(f"提掉棋子: ({captured['row']+1},{captured['col']+1})")
                self.board[captured['row']][captured['col']] = 0  # 设置为空
                self.move_numbers[captured['row']][captured['col']] = 0  # 清除步数
            
            # 更新提子计数
            if stone == 2:  # 白棋提子，黑子被提
                self.black_captured += len(captured_stones)
            else:  # 黑棋提子，白子被提
                self.white_captured += len(captured_stones)
            
            # 检查是否形成劫争
            if len(captured_stones) == 1:
                new_ko_point = self.check_ko(row, col, stone, captured_stones)
                if new_ko_point:
                    # 形成劫争，设置劫点
                    self.ko_point = new_ko_point
                    print(f"形成劫争，劫点: ({self.ko_point['row']+1},{self.ko_point['col']+1})")
                else:
                    # 不是劫争，清除劫点
                    self.ko_point = None
            else:
                # 提了多个子，不是劫，清除劫点
                self.ko_point = None
        else:
            # 没有提子，清除劫点
            self.ko_point = None
        
        # 检查落子后自身是否还有气（防止自杀）
        if self.count_liberties(row, col) == 0:
            # 自杀，移除这个子
            self.board[row][col] = 0
            self.move_numbers[row][col] = 0
            self.current_move_number -= 1
            print("自杀禁止，移除棋子")
            return False
        
        # 清除地盘显示
        self.clear_territory()
        
        self.save_game_state()
        
        # 切换回合
        self.black_turn = not self.black_turn
        
        return True
    
    def calculate_territory(self):
        """计算地盘"""
        # 初始化地盘棋盘
        for i in range(self.board_size):
            self.territory_board[i] = [0] * self.board_size  # 0: 未标记, 1: 黑方地盘, 2: 白方地盘, 3: 中立区域
        
        visited = [[False] * self.board_size for _ in range(self.board_size)]
        
        # 遍历所有空点
        for i in range(self.board_size):
            for j in range(self.board_size):
                if self.board[i][j] == 0 and not visited[i][j]:
                    territory = self.flood_fill_territory(i, j, visited)
                    
                    if territory['owner'] == 1:
                        # 黑方地盘
                        for cell in territory['cells']:
                            self.territory_board[cell['row']][cell['col']] = 1
                    elif territory['owner'] == 2:
                        # 白方地盘
                        for cell in territory['cells']:
                            self.territory_board[cell['row']][cell['col']] = 2
                    else:
                        # 中立区域（双活）
                        for cell in territory['cells']:
                            self.territory_board[cell['row']][cell['col']] = 3
        
        # 统计地盘数量
        black_count = white_count = neutral_count = 0
        for i in range(self.board_size):
            for j in range(self.board_size):
                if self.territory_board[i][j] == 1:
                    black_count += 1
                elif self.territory_board[i][j] == 2:
                    white_count += 1
                elif self.territory_board[i][j] == 3:
                    neutral_count += 1
        
        print(f"地盘统计 - 黑方: {black_count}, 白方: {white_count}, 中立: {neutral_count}")
        
        return self.territory_board
    
    def flood_fill_territory(self, start_row, start_col, visited):
        """洪水填充法确定地盘归属"""
        cells = []
        borders = set()  # 使用set来去重
        stack = [{'row': start_row, 'col': start_col}]
        
        while stack:
            current = stack.pop()
            
            if visited[current['row']][current['col']]:
                continue
            
            visited[current['row']][current['col']] = True
            cells.append(current)
            
            directions = [(-1, 0), (1, 0), (0, -1), (0, 1)]
            for dr, dc in directions:
                nr, nc = current['row'] + dr, current['col'] + dc
                
                if self.is_valid_position(nr, nc):
                    if self.board[nr][nc] == 0 and not visited[nr][nc]:
                        stack.append({'row': nr, 'col': nc})
                    elif self.board[nr][nc] != 0:
                        # 记录边界上的棋子颜色
                        borders.add(self.board[nr][nc])
        
        # 确定地盘归属
        owner = 0  # 0: 中立, 1: 黑方, 2: 白方
        
        if len(borders) == 1:
            # 只有一种颜色的边界棋子
            if 1 in borders:
                owner = 1  # 黑方地盘
            elif 2 in borders:
                owner = 2  # 白方地盘
        elif len(borders) > 1:
            # 有多种颜色的边界棋子，是中立区域（双活）
            owner = 0
        
        return {
            'cells': cells,
            'owner': owner
        }
    
    def calculate_score(self, komi):
        """计算分数并判断胜负"""
        # 先计算地盘
        self.calculate_territory()
        
        black_score = 0
        white_score = 0
        
        # 计算地盘分数
        for i in range(self.board_size):
            for j in range(self.board_size):
                if self.territory_board[i][j] == 1:
                    black_score += 1  # 黑方地盘
                elif self.territory_board[i][j] == 2:
                    white_score += 1  # 白方地盘
                # 中立区域不计分
        
        # 加上提子分数
        black_score += self.white_captured
        white_score += self.black_captured
        
        # 白方加上贴目
        white_score += komi
        
        # 判断胜负
        if black_score > white_score:
            winner = "黑方胜"
        elif white_score > black_score:
            winner = "白方胜"
        else:
            winner = "和棋"
        
        return {
            'winner': winner,
            'black_score': black_score,
            'white_score': white_score,
            'black_territory': black_score - self.black_captured,
            'white_territory': white_score - self.white_captured - komi,
            'komi': komi
        }
    
    # 修复 update_status 方法，确保返回正确的数据类型
    def update_status(self):
        """更新游戏状态"""
        turn_text = "黑方" if self.black_turn else "白方"
        ko_text = " | 劫争进行中" if self.ko_point else ""
        undo_text = " | 右键悔棋" if len(self.move_history) > 1 else ""
        move_number_text = f" | 步数: {self.current_move_number}"
        show_numbers_text = " | 显示步数" if self.show_move_numbers else " | 隐藏步数"
        show_territory_text = " | 显示地盘" if self.show_territory else " | 隐藏地盘"
        status = (f"{turn_text}{ko_text}{move_number_text} | 黑被提子: {self.black_captured} | "
                f"白被提子: {self.white_captured}{undo_text}{show_numbers_text}{show_territory_text}")
        
        # 确保返回的字典有所有必需的键
        ko_dict = None
        if self.ko_point:
            ko_dict = {'row': self.ko_point.get('row', -1), 'col': self.ko_point.get('col', -1)}
        
        territory = []
        if hasattr(self, 'territory_board') and self.territory_board:
            territory = self.territory_board
        else:
            # 如果没有地盘棋盘，创建空的
            territory = [[0] * self.board_size for _ in range(self.board_size)]
        
        return {
            'text': status,
            'black_turn': self.black_turn,
            'current_move_number': self.current_move_number,
            'black_captured': self.black_captured,
            'white_captured': self.white_captured,
            'ko_point': ko_dict,  # 确保不是 None 对象
            'show_move_numbers': self.show_move_numbers,
            'show_territory': self.show_territory,
            'territory_board': territory
        }



def create_game_state(board_size):
    """创建游戏状态对象"""
    return GameState(board_size)