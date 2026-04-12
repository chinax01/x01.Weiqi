import copy
import random


from katrain.core.sgf_parser import Move  # noqa: E402
from katrain.core.base_katrain import KaTrainBase, PLAYER_HUMAN, PLAYING_NORMAL  # noqa: E402
from katrain.core.engine import KataGoEngine  # noqa: E402
from katrain.core.game import Game  # noqa: E402
from katrain.core.ai import OUTPUT_ERROR, generate_ai_move, AI_DEFAULT  # noqa: E402
from katrain.core.constants import PLAYER_AI  # noqa: E402


MAX_VISITS = 200


class KaTrain(KaTrainBase):
    def __init__(self, force_package_config=False, debug_level=None, **kwargs):
        super().__init__(
            force_package_config=force_package_config, debug_level=debug_level, **kwargs
        )
        self.engine = None
        self.contributing = False
        self.pondering = False
        self.show_move_num = False

    def start(self):
        if self.engine:
            return
        self.engine = KataGoEngine(self, self.config("engine"))
        self._do_new_game()

    def _do_new_game(self, move_tree=None, analyze_fast=False, sgf_filename=None):
        self.pondering = False

        if self.engine:
            self.engine.on_new_game()  # clear queries

        self.game = Game(
            self,
            self.engine,
            move_tree=move_tree,
            analyze_fast=analyze_fast or not move_tree,
            sgf_filename=sgf_filename,
        )

        for bw, player_info in self.players_info.items():
            player_info.sgf_rank = self.game.root.get_property(bw + "R")
            player_info.calculated_rank = None
            if sgf_filename is not None:  # load game->no ai player
                player_info.player_type = PLAYER_HUMAN
                player_info.player_subtype = PLAYING_NORMAL
            self.update_player(
                bw,
                player_type=player_info.player_type,
                player_subtype=player_info.player_subtype,
            )

    def __call__(self, message, *args, **kwargs):
        if message == "engine_recovery_popup":
            error_message = args[0] if args else "Unknown error"
            code = args[1] if len(args) > 1 else None
            self.log(
                f"Engine recovery needed: {error_message} (code: {code})", OUTPUT_ERROR
            )
            return True
        elif message == "update_state":
            # Update game state
            self._do_update_state()
        else:
            self.log(f"Unhandled message: {message} with args: {args}", OUTPUT_DEBUG)

    def _do_update_state(self):
        """Update game state - simplified version"""
        if not self.game or not self.game.current_node:
            return

        cn = self.game.current_node
        if not self.contributing:
            _last_player, next_player = (
                self.players_info[cn.player],
                self.players_info[cn.next_player],
            )

            if (
                cn.analysis_complete
                and next_player.ai
                and not cn.children
                and not self.game.end_result
                and cn.auto_undo is None
            ):
                self._do_ai_move(cn)

            # Pondering logic
            if self.pondering:
                self.game.analyze_extra("ponder")
            elif self.engine:
                self.engine.stop_pondering()

    def _do_ai_move(self, node=None):
        """Execute AI move - simplified version"""
        if node is None or self.game.current_node == node:
            mode = self.next_player_info.strategy
            settings = self.config(f"ai/{mode}")
            if settings is not None:
                from katrain.core.ai import generate_ai_move

                generate_ai_move(self.game, mode, settings)
            else:
                self.log(f"AI Mode {mode} not found!", OUTPUT_ERROR)


class GoAI:
    def __init__(self, game_state, difficulty="hard"):
        self.game_state = game_state
        self.difficulty = difficulty
        self.board_size = (
            game_state.board_size if hasattr(game_state, "board_size") else 19
        )
        self.invalid_positions = []
        self.katrain = KaTrain(debug_level=0)
        self.katrain.start()

    def get_legal_moves(self, stone):
        """Get all legal moves for current player"""
        legal_moves = []
        for row in range(self.board_size):
            for col in range(self.board_size):
                if self._is_valid_move(row, col, stone):
                    legal_moves.append((row, col))
        return legal_moves

    def _is_valid_move(self, row, col, stone):
        # 边界检查
        if not (0 <= row < self.board_size and 0 <= col < self.board_size):
            return False

        board = getattr(self.game_state, "board", [])
        if (
            not board
            or row >= len(board)
            or col >= len(board[0])
            or board[row][col] != 0
        ):
            return False

        # 劫点检查
        ko_point = getattr(self.game_state, "ko_point", None)
        if ko_point and isinstance(ko_point, dict):
            if row == ko_point.get("row", -1) and col == ko_point.get("col", -1):
                return False

        # 复制棋盘进行模拟
        temp_board = copy.deepcopy(board)
        opponent = 2 if stone == 1 else 1

        # 落子
        temp_board[row][col] = stone

        # 反复提走所有无气的对方棋子
        while True:
            removed = False
            for r in range(self.board_size):
                for c in range(self.board_size):
                    if temp_board[r][c] == opponent:
                        # BFS 找出整个对方组并检查是否有气
                        visited = set()
                        stack = [(r, c)]
                        has_liberty = False
                        group_members = []
                        while stack:
                            cr, cc = stack.pop()
                            if (cr, cc) in visited:
                                continue
                            visited.add((cr, cc))
                            group_members.append((cr, cc))
                            for dr, dc in [(-1, 0), (1, 0), (0, -1), (0, 1)]:
                                nr, nc = cr + dr, cc + dc
                                if (
                                    0 <= nr < self.board_size
                                    and 0 <= nc < self.board_size
                                ):
                                    if temp_board[nr][nc] == 0:
                                        has_liberty = True
                                        break
                                    elif (
                                        temp_board[nr][nc] == opponent
                                        and (nr, nc) not in visited
                                    ):
                                        stack.append((nr, nc))
                            if has_liberty:
                                break
                        if not has_liberty:
                            # 无气则移除整组
                            for gr, gc in group_members:
                                temp_board[gr][gc] = 0
                            removed = True
            if not removed:
                break

        # 检查落子所在组是否有气
        visited = set()
        stack = [(row, col)]
        has_liberty = False
        while stack:
            r, c = stack.pop()
            if (r, c) in visited:
                continue
            visited.add((r, c))
            for dr, dc in [(-1, 0), (1, 0), (0, -1), (0, 1)]:
                nr, nc = r + dr, c + dc
                if 0 <= nr < self.board_size and 0 <= nc < self.board_size:
                    if temp_board[nr][nc] == 0:
                        has_liberty = True
                        break
                    elif temp_board[nr][nc] == stone and (nr, nc) not in visited:
                        stack.append((nr, nc))
            if has_liberty:
                break

        return has_liberty

    def easy_move(self, legal_moves):
        """Easy difficulty: random move"""
        return random.choice(legal_moves) if legal_moves else None

    def medium_move(self, legal_moves, stone):
        """Medium difficulty: simple heuristic"""
        if not legal_moves:
            return None

        # Prefer center and connections
        scored = []
        center = (self.board_size - 1) / 2.0

        for r, c in legal_moves:
            score = 0

            # Prefer center
            dist = ((r - center) ** 2 + (c - center) ** 2) ** 0.5
            score += 20 - dist

            # Prefer connecting to own stones
            for dr, dc in [(-1, 0), (1, 0), (0, -1), (0, 1)]:
                nr, nc = r + dr, c + dc
                if 0 <= nr < self.board_size and 0 <= nc < self.board_size:
                    if getattr(self.game_state, "board", [])[nr][nc] == stone:
                        score += 3

            scored.append((score, (r, c)))

        scored.sort(reverse=True)
        if scored:
            # 80% chance to pick best, 20% to pick random from top 3
            if random.random() < 0.8:
                return scored[0][1]
            else:
                top_n = min(3, len(scored))
                return random.choice(scored[:top_n])[1]

        return random.choice(legal_moves)

    def hard_move(self, legal_moves, stone):
        if not legal_moves:
            return None

        try:
            katrain = self.katrain
            if self.game_state.move_history:
                move = self.game_state.move_history[-1]
                if move:
                    player = "B" if move["stone"] == 1 else "W"
                    coords = (int(move["row"]), int(move["col"]))
                    # 关键：非如此不能和 AI 交互
                    katrain.game.play(move=Move(coords, player))

            # 配置 AI 为当前玩家
            player = "B" if stone == 1 else "W"
            katrain.update_player(
                player, player_type=PLAYER_AI, player_subtype=AI_DEFAULT
            )

            ai_settings = katrain._config["ai"][AI_DEFAULT]

            move_obj, played_node = generate_ai_move(
                katrain.game, AI_DEFAULT, ai_settings
            )

            if move_obj and not move_obj.is_pass:
                print(f"[GoAI] KaTrain 返回的落子: {move_obj.coords}")
                row, col = move_obj.coords
                move = (row, col)
                if move in legal_moves:
                    print(f"[GoAI] KaTrain 选择落子: {move_obj.gtp()} -> {move}")
                    return move
            else:
                print(f"[GoAI] KaTrain 没有找到落子: {move_obj}")
        except Exception as e:
            print(f"[GoAI] KaTrain 分析出错: {e}")

        print("[GoAI] 所有高级方法失败，使用中等难度")
        return self.medium_move(legal_moves, stone)

    def get_best_move(self):
        stone = 2 if getattr(self.game_state, "black_turn", True) else 1
        legal_moves = self.get_legal_moves(stone)
        if not legal_moves:
            return None

        # 过滤已知无效位置
        filtered_moves = [m for m in legal_moves if m not in self.invalid_positions]
        if not filtered_moves:
            self.invalid_positions.clear()
            filtered_moves = legal_moves

        if self.difficulty == "easy":
            return self.easy_move(filtered_moves)
        elif self.difficulty == "medium":
            return self.medium_move(filtered_moves, stone)
        else:  # hard
            return self.hard_move(filtered_moves, stone)
