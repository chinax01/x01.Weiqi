# `katrain/core/constants.py`

### **1. 程序基本信息**
- `PROGRAM_NAME`, `VERSION`, `HOMEPAGE`：程序名称、版本号及项目主页。
- `CONFIG_MIN_VERSION`, `ANALYSIS_FORMAT_VERSION`：配置文件兼容的最低版本、分析数据格式版本。
- `DATA_FOLDER`：用户数据存储目录（默认 `~/.katrain`）。

### **2. 日志与输出级别**
- `OUTPUT_ERROR`～`OUTPUT_EXTRA_DEBUG`：定义不同详细程度的日志输出等级（错误、标准错误、信息、调试、额外调试）。
- `KATAGO_EXCEPTION`：标识 KataGo 内部错误的特殊字符串。

### **3. 消息与任务优先级**
- `STATUS_ANALYSIS`～`STATUS_ERROR`：用于区分不同状态消息的优先级（分析、信息、教学、错误）。
- `PRIORITY_GAME_ANALYSIS`～`PRIORITY_EXTRA_AI_QUERY`：控制分析队列中任务的执行顺序（如对局分析优先级最低，额外查询最高）。

### **4. 玩家与游戏模式**
- `PLAYER_HUMAN`, `PLAYER_AI`：标识玩家类型（人类或 AI）。
- `PLAYING_NORMAL`, `PLAYING_TEACHING`：游戏模式（正常对局或教学模式）。
- `MODE_PLAY`, `MODE_ANALYZE`：程序工作模式（对局或分析）。

### **5. AI 策略标识符**
- `AI_DEFAULT`, `AI_HANDICAP`, `AI_SCORELOSS` 等：内置的 AI 下棋策略名称，例如：
  - `AI_DEFAULT`：基础策略
  - `AI_HANDICAP`：让子局策略
  - `AI_SCORELOSS`：倾向避免大损失
  - `AI_PICK`, `AI_LOCAL`, `AI_TENUKI` 等：基于选点或局部倾向的策略
  - `AI_HUMAN`, `AI_PRO`：模仿人类或职业棋手的风格
- 列表 `AI_STRATEGIES`, `AI_STRATEGIES_RECOMMENDED_ORDER`：汇总所有策略及其推荐显示顺序。

### **6. AI 强度与参数映射**
- `AI_STRENGTH`：各策略对应的近似段位（用于无模型时的后备显示）。
- `AI_OPTION_VALUES`：为各策略参数提供的可选数值（如等级、强度系数、选点比例等）。
- `AI_KEY_PROPERTIES`：用于配置文件的关键属性集合。
- `CALIBRATED_RANK_ELO`：段位与等级分（ELO）的对应关系。
- `AI_*_ELO` 及 `AI_*_ELO_GRID`：部分策略（如加权、得分损失、选点等）的参数与 ELO 的映射网格，用于估计强度。

### **7. 顶部分析选项**
- `TOP_MOVE_*`：定义在分析时如何比较最佳着法（如基于得分差、胜率差、访问量等）。
- `TOP_MOVE_OPTIONS`：可供用户选择的顶部分析模式列表。

### **8. 时间与界面刷新**
- `REPORT_DT`, `PONDERING_REPORT_DT`：状态报告和后台思考时的界面更新间隔（秒）。

### **9. SGF 特殊标记**
- `SGF_INTERNAL_COMMENTS_MARKER`, `SGF_SEPARATOR_MARKER`：用于在 SGF 棋谱中嵌入内部数据的分隔符，避免与用户注释混淆。

这些常量集中管理了 KaTrain 的核心配置、策略定义、优先级及数据格式，便于程序内部统一调用和维护。

# `katrain/core/lang.py`

这段代码实现了一个多语言国际化（i18n）系统，主要用于KataGo的图形界面应用。让我详细解释其主要组件和功能：

## 主要功能

### 1. **Lang 类**
继承自 `kivy._event.Observable`，实现了观察者模式的语言管理类。

#### 核心属性：
- `observers`: 存储需要语言更新通知的UI组件
- `callbacks`: 存储语言切换时的回调函数
- `FONTS`: 不同语言对应的字体文件映射

#### 主要方法：

**初始化方法 `__init__`**
```python
def __init__(self, lang):
    super(Lang, self).__init__()
    self.lang = lang 
    self.switch_lang(lang)
```

**翻译方法 `_`**
- 简化的翻译调用接口
- 调用 `ugettext` 进行实际的文本翻译

**字体设置 `set_widget_font`**
- 为控件及其子控件（提示标签、消息标签）设置对应的语言字体

**绑定方法 `fbind`**
- 重写父类的绑定方法
- 当绑定翻译属性时，将控件添加到观察者列表
- 自动设置控件的字体

**解绑方法 `funbind`**
- 从观察者列表中移除控件

**语言切换 `switch_lang`**
- 核心功能：切换应用程序语言
- 更新当前语言和字体
- 加载对应的翻译文件（使用 `gettext`）
- 通知所有观察者更新文本
- 执行所有回调函数

### 2. **国际化实现细节**

```python
i18n_dir, _ = os.path.split(find_package_resource('katrain/i18n/__init__.py'))
local_dir = os.path.join(i18n_dir, 'locales')
locales = gettext.translation('katrain', local_dir, languages=[lang, DEFAULT_LANGUAGE])
self.ugettext = locales.gettext
```

- 使用Python标准库 `gettext` 实现翻译
- 从 `locales` 目录加载对应语言的翻译文件
- 支持多语言（默认语言为英语）

### 3. **工具函数**

**段级位标签转换 `rank_label`**
```python
def rank_label(rank):
    if rank is None:
        return '??K'
    if rank >= 0.5:
        return f'{rank:.0f}{i18n._('strength:dan')}'
    else:
        return f'{1-rank:.0f}{i18n._("strength:kyu")}'
```
- 将围棋等级分数转换为显示标签
- >0.5 显示为段位（dan）
- <0.5 显示为级位（kyu）
- 使用当前语言的翻译

### 4. **全局实例**

```python
DEFAULT_LANGUAGE = 'en'
i18n = Lang(DEFAULT_LANGUAGE)
```
- 创建默认语言的全局语言实例

## 设计特点

1. **观察者模式**：当语言切换时自动更新所有相关UI组件
2. **字体自适应**：根据语言自动切换合适的字体
3. **资源管理**：通过 `find_package_resource` 定位翻译文件
4. **错误处理**：包含异常处理，避免语言切换时的崩溃

这个系统使得应用程序能够在不重启的情况下动态切换语言，同时保持UI的实时更新。

# `katrain/core/utils.py`

这段代码是一个工具函数集合（utils.py），提供了多种实用的辅助功能。让我详细解释每个函数的作用：

## 主要功能模块

### 1. **资源路径查找 `find_package_resource`**

```python
def find_package_resource(path: str, silent_errors=False):
```
**功能**：定位包内资源的绝对路径

**工作流程**：
- 如果是 `katrain` 包内的资源（路径以 `katrain` 开头）
  - 首次调用时获取包的安装路径并缓存
  - 拼接完整的资源路径
- 如果是外部路径，直接返回绝对路径

**用途**：确保程序能找到需要的资源文件（如翻译文件、字体、配置文件等）

**错误处理**：如果包路径不存在，打印错误但不崩溃

### 2. **浮点数序列化**

**打包 `pack_floats`**
```python
def pack_floats(float_list):
    return struct.pack(f'{len(float_list)}e', *float_list)
```
- 将浮点数列表转换为二进制数据
- 使用 `'e'` 格式（4字节浮点数）
- 用于网络传输或文件存储

**解包 `unpack_floats`**
```python
def unpack_floats(s, num):
    return struct.unpack(f'{num}e', s)
```
- 从二进制数据还原浮点数列表
- 需要指定浮点数的数量

### 3. **访问量格式化 `format_visits`**

```python
def format_visits(n):
    if n < 1000: return str(n)           # 小于1000：直接显示
    if n < 1e5: return f'{n/1000:.1f}K'   # 小于10万：显示K（千），带1位小数
    if n < 1e6: return f'{n/1000:.0f}K'   # 小于100万：显示K，无小数
    return f'{n/1e6:.0f}M'                 # 大于100万：显示M（百万）
```

**示例**：
- 500 → "500"
- 1500 → "1.5K"
- 45000 → "45.0K"
- 123456 → "123K"
- 2000000 → "2M"

### 4. **JSON数据截断 `json_truncate_arrays`**

```python
def json_truncate_arrays(data, lim=20):
```
**功能**：递归地截断大型JSON数组，便于日志记录或显示

**处理规则**：
- 如果是列表且包含字典：递归处理每个字典
- 如果是列表且超过长度限制：替换为描述性文本
- 如果是字典：递归处理每个值
- 其他类型：直接返回

**示例**：
```python
# 长列表 [0.1, 0.2, 0.3, ... 100个] 
# 变为 ["100 x float"]
```

### 5. **带权重的随机抽样 `weighted_selection_without_replacement`**

```python
def weighted_selection_without_replacement(items: List[Tuple], pick_n: int) -> List[Tuple]:
```
**功能**：从列表中按权重随机选择指定数量的项，不重复

**算法原理**：
```python
elt = [(math.log(random.random()) / (item[1] + 1e-18), item) for item in items]
```
- 使用"指数变换"技巧实现加权随机抽样
- 每个项计算一个随机得分 = log(随机数) / 权重
- 选择得分最高的 `pick_n` 个项

**特点**：
- 权重越大的项被选中的概率越高
- 不会重复选择同一个项
- 使用堆排序算法（`heapq.nlargest`）高效获取最大项
- 包含小量 `1e-18` 避免除零错误

**应用场景**：
- MCTS（蒙特卡洛树搜索）中的节点选择
- 推荐系统中的加权采样
- 概率性算法

## 整体设计特点

1. **性能优化**：使用缓存（PATHS字典）、高效算法（堆排序）
2. **容错性**：处理各种异常情况，提供默认值
3. **实用性**：解决实际问题（格式化、截断、序列化）
4. **数学应用**：在随机抽样中巧妙使用对数变换

这些工具函数为KataGo的核心功能提供了基础支持，特别是在处理围棋AI的大量数据时非常实用。

# `katrain/core/sgf_parser.py`

这段代码是一个**SGF（Smart Game Format）围棋棋谱解析器**，主要用于解析和处理围棋对局文件。让我详细解释主要组件：

## 核心类结构

### 1. **Move类** - 表示落子
- 处理坐标转换（GTP格式 ↔ SGF格式）
- 支持Pass（虚着）
- 坐标系统支持最大52路棋盘

### 2. **SGFNode类** - 表示棋谱节点
每个节点代表一个对局状态，包含：
- **属性存储**：使用`defaultdict`存储SGF属性（如B、W、KM、HA等）
- **树形结构**：通过`parent`和`children`形成游戏树
- **重要属性**：
  - `board_size`：棋盘大小
  - `komi`：贴目
  - `handicap`：让子数
  - `moves`：当前节点的落子
  - `placements`：棋盘上的棋子（包括让子）

### 3. **SGF类** - 主解析器
负责解析不同格式的棋谱文件：
- **SGF格式**：标准围棋棋谱格式
- **NGF格式**：特定程序使用的格式
- **GIB格式**：另一种棋谱格式

## 主要功能

### 解析功能
```python
# 解析SGF字符串
root = SGF.parse_sgf("(;FF[4]SZ[19]...")

# 解析文件
root = SGF.parse_file("game.sgf")
```

### 节点操作
```python
# 获取当前节点信息
moves = node.moves  # 当前节点的落子
placements = node.placements  # 棋盘上的棋子
player = node.player  # 当前轮到谁

# 在游戏中前进
next_node = node.play(move)  # 根据落子进入下一节点
```

### 特殊功能
- **自动检测编码**：使用chardet检测文件编码
- **坐标展开**：处理SGF中的矩形选区（如"aa:dd"）
- **让子处理**：自动放置让子（`place_handicap_stones`方法）
- **FoxWQ兼容**：修正特定平台的贴目值

## 错误处理
- 自定义`ParseError`异常
- 多种格式的容错解析
- 坐标范围检查

这个解析器功能完善，支持多种围棋文件格式，是围棋AI和分析工具的重要基础组件。

## SGFPROP_PAT = re.compile(r"\s*(?:\(|\)|;|(\w+)((\s*\[([^\]\\]|\\.)*\])+))", flags=re.DOTALL)

这个正则表达式用于解析 SGF（Smart Game Format）棋谱文件中的基本语法单元。SGF 文件由括号 `()`、分号 `;` 和属性（Property）组成，属性格式为 `属性名[值1][值2]...`，其中值可以包含转义字符（反斜杠 `\` 转义）。下面逐部分解释该正则表达式：

```python
SGFPROP_PAT = re.compile(
    r"\s*(?:\(|\)|;|(\w+)((\s*\[([^\]\\]|\\.)*\])+))",
    flags=re.DOTALL
)
```

### 1. 模式结构概览
- `\s*`：匹配任意空白字符（包括空格、制表符、换行等）0次或多次，用于跳过空白。
- `(?: ... )`：非捕获分组，包含多个备选项，用 `|` 分隔。匹配其中任意一项，但不单独捕获该分组内容。
  - 备选项1：`\(` 匹配左括号 `(`
  - 备选项2：`\)` 匹配右括号 `)`
  - 备选项3：`;` 匹配分号
  - 备选项4：`(\w+)((\s*\[([^\]\\]|\\.)*\])+)` 匹配 SGF 属性

### 2. 属性匹配部分详解
这是最复杂的部分，用于匹配形如 `B[aa]`、`KM[6.5]` 或 `AB[dd][ee][ff]` 的属性。

- `(\w+)`：**捕获组1**，匹配一个或多个单词字符（字母、数字、下划线），即属性名，如 `B`、`KM`、`AB`。
- `((\s*\[([^\]\\]|\\.)*\])+)`：**捕获组2**，匹配一个或多个用方括号包围的值。
  - `+` 表示前面的整体（一个方括号值）重复一次或多次。
  - 每个方括号值的结构是：`\s*\[([^\]\\]|\\.)*\]`
    - `\s*`：值前可能存在的空白（SGF 标准允许值前有空格吗？通常没有，但兼容处理）。
    - `\[`：匹配左方括号 `[`。
    - `([^\]\\]|\\.)*`：匹配方括号内的内容。这是一个重复0次或多次的选择结构：
      - `[^\]\\]`：匹配任何不是 `]` 和 `\` 的字符。
      - `\\.`：匹配一个反斜杠后跟任意字符（转义序列），如 `\]` 或 `\\` 本身。这确保转义字符被正确识别，不会提前结束方括号。
    - `\]`：匹配右方括号 `]`。

由于使用了 `re.DOTALL` 标志，`.` 可以匹配换行符，因此属性值可以跨越多行（虽然罕见）。

### 3. 捕获组说明
- 组1：属性名（如 `B`、`KM`）。
- 组2：整个值列表字符串（例如 `[aa][bb]` 整体）。
- 组3：实际上 `([^\]\\]|\\.)` 也是一个捕获组，但通常不需要直接使用，因为值内容还需进一步解析（去除转义）。

### 4. 在解析器中的用途
在 `SGF` 类的 `_parse_branch` 方法中，该正则表达式被反复应用于当前解析位置之后的字符串，提取出下一个语法单元：
- 如果匹配到 `(` 或 `)`，则处理分支开始或结束。
- 如果匹配到 `;`，则表示新节点的开始。
- 如果匹配到属性，则获取属性名（组1）和整个值字符串（组2），然后通过 `re.split(r'\]\s*\[', value)` 拆分成多个单独的值，并对每个值进行转义还原（`_unescape_value`）。

### 5. 设计要点
- **非捕获分组**：避免干扰捕获组编号，只关心属性名和值列表。
- **转义处理**：`([^\]\\]|\\.)*` 确保转义字符（如 `\]`）不会错误地结束方括号，同时允许值内包含反斜杠。
- **空白容忍**：`\s*` 允许在属性名和值前有空白，增强容错性。

这个正则表达式是 SGF 解析器的核心词法分析部分，能够准确识别 SGF 文件中的关键标记，为后续语法分析提供基础。

## SGFNode

`SGFNode` 类是 SGF 解析器中的核心数据结构，代表棋谱树中的一个节点。每个节点对应一个对局状态，包含该节点的属性（如落子、注释、标记等）、子节点列表（后续变化）以及父节点引用。下面详细解释其各个组成部分。

### 构造函数 `__init__`

```python
def __init__(self, parent: "SGFNode" = None, properties=None, move: Move = None):
    self.children = []
    self.properties = defaultdict(list)
    if properties:
        for k, v in properties.items():
            self.set_property(k, v)
    self.parent = parent
    if self.parent:
        self.parent.children.append(self)
    if parent and move:
        self.set_property(move.player, move.sgf(self.board_size))
    self._clear_cache()
```

- **`self.children`**：子节点列表，初始为空。每个节点可以有多个子节点，对应不同的分支（变化）。
- **`self.properties`**：使用 `defaultdict(list)` 存储 SGF 属性。属性名作为键，值始终为列表（即使单个值也包装为列表），以支持多值属性（如 `AB[aa][bb]`）。
- 如果传入 `properties` 字典，则遍历并调用 `set_property` 添加。
- 设置父节点：将 `self.parent` 赋值为传入的 `parent`；如果父节点存在，则将自身追加到父节点的 `children` 列表中。
- 如果同时提供了 `parent` 和 `move`，则根据当前节点的棋盘大小（通过 `self.board_size` 获取）将 `move` 转换为 SGF 坐标字符串，并以 `move.player` 为属性名（如 `"B"` 或 `"W"`）设置到 `properties` 中。这通常用于创建表示一步棋的节点。
- 最后调用 `_clear_cache()` 初始化缓存。

### 缓存管理 `_clear_cache`

```python
def _clear_cache(self):
    self.moves_cache = None
```

清空 `moves_cache`，该缓存用于存储解析后的 `Move` 对象列表。当节点属性发生变化时（如添加或修改属性），需要清空缓存以确保下次获取时重新解析。

### 字符串表示 `__repr__`

```python
def __repr__(self):
    return f"SGFNOde({dict(self.properties)})"
```

返回节点的简易字符串表示，显示其属性字典。注意拼写错误 `"SGFNOde"`，但通常不影响调试。

### 属性深拷贝 `sgf_properties`

```python
def sgf_properties(self, **kargs) -> Dict:
    return copy.deepcopy(self.properties)
```

返回节点属性的深拷贝，避免外部修改影响内部数据。`**kargs` 未使用，可能是为了子类扩展。

### 子节点排序 `order_children` 和 `ordered_children`

```python
@staticmethod
def order_children(children):
    return children

@property
def ordered_children(self):
    return self.order_children(self.children)
```

`order_children` 是一个静态方法，默认返回原列表，子类可重写以实现自定义排序（如按某种规则排列变化）。`ordered_children` 属性调用该方法，用于获取排序后的子节点列表。

### 转义处理 `_escape_value` 和 `_unescape_value`

```python
@staticmethod
def _escape_value(value):
    return re.sub(r"([\]\\])", r"\\\1", value) if isinstance(value, str) else value

@staticmethod
def _unescape_value(value):
    return re.sub(r"\\([\]\\])", r"\1", value) if isinstance(value, str) else value
```

SGF 属性值中，字符 `]` 和 `\` 需要转义（在 `]` 前加 `\`，`\` 本身也需转义为 `\\`）。这两个方法分别用于生成 SGF 字符串时转义，以及解析时还原转义。注意它们只处理字符串，非字符串原样返回。

### 生成 SGF 字符串 `sgf`

```python
def sgf(self, **kargs) -> str:
    def node_sgf_str(node):
        return ";" + "".join(
            [
                prop + "".join(f"[{self._escape_value(v)}]" for v in values)
                for prop, values in node.sgf_properties(**kargs).items()
                if values
            ]
        )

    stack = [")", self, "("]
    sgf_str = ""
    while stack:
        item = stack.pop()
        if isinstance(item, str):
            sgf_str += item
        else:
            sgf_str += node_sgf_str(item)
            if len(item.children) == 1:
                stack.append(item.children[0])
            elif item.children:
                stack += sum(
                    [[")", c, "("] for c in item.ordered_children[::-1]],
                    []
                )
    return sgf_str
```

将当前节点及其子树递归转换为标准的 SGF 字符串格式。算法使用栈模拟树的遍历（后序遍历），具体：
- 定义内部函数 `node_sgf_str`，将单个节点转换为 `;` 开头的属性字符串（如 `;B[aa]C[comment]`）。属性值通过 `_escape_value` 转义。
- 初始栈内容为 `[")", self, "("]`，表示从根节点开始，准备输出 `(; ... )`。
- 循环弹出栈顶：
  - 如果弹出的是字符串，直接拼接到结果。
  - 如果是节点，先输出该节点的字符串，然后根据子节点数量决定入栈方式：
    - 如果只有一个子节点，直接将该子节点入栈（后续继续处理）。
    - 如果有多个子节点，则按逆序（为了保持正确的括号嵌套）将每个子节点及其前后的 `)` 和 `(` 压入栈，这样会输出多个分支：`( ... )( ... )`。
- 最终得到类似 `(;B[aa];W[bb](;B[cc])(;B[dd]))` 的字符串。

`**kargs` 传递给 `sgf_properties`，可能用于控制属性过滤等。

### 属性操作方法

#### `add_list_property(property: str, values: list)`

```python
def add_list_property(self, property:str, values:list):
    normalized_property = re.sub('[a-z]', '', property)
    self._clear_cache()
    self.properties[normalized_property] += values
```

添加属性值到现有列表。先对属性名进行“归一化”：删除所有小写字母（SGF 属性名通常为大写，但有时解析可能得到小写，这里确保属性名统一为大写）。然后清空缓存，并将 `values` 列表追加到该属性对应的列表中。

#### `get_list_property(property, default=None) -> Any`

```python
def get_list_property(self, property, default=None) -> Any:
    return self.properties.get(property, default)
```

返回属性对应的整个列表，若属性不存在则返回 `default`（默认为 `None`）。

#### `set_property(property: str, value: Any)`

```python
def set_property(self, property:str, value: Any):
    if not isinstance(value, list):
        value = [value]
    self._clear_cache()
    self.properties[property] = value
```

设置属性值。如果 `value` 不是列表，则包装为列表。清空缓存，然后赋值。

#### `get_property(property: str, default=None) -> Any`

```python
def get_property(self, property:str, default=None) -> Any:
    return self.properties.get(property, [default])[0]
```

获取属性的第一个值（因为 SGF 中大多数属性只用一个值）。如果属性不存在，返回 `default`。注意：如果属性存在但列表为空，可能返回 `default` 也可能出错，但通常不会发生。

#### `clear_perperty(property) -> Any`（拼写错误应为 `clear_property`）

```python
def clear_perperty(self, property) -> Any:
    return self.properties.pop(property, None)
```

移除属性并返回其值（或 `None` 如果不存在）。注意方法名拼写错误，但功能正常。

### 父节点属性与缓存重置

```python
@property
def parent(self) -> Optional['SGFNode']:
    return self._parent

@parent.setter
def parent(self, parent_node):
    self._parent = parent_node
    self._root = None
    self._depth = None
```

使用属性装饰器管理 `_parent`，当设置父节点时，同时清空 `_root` 和 `_depth` 缓存（因为根节点和深度可能变化）。

### 根节点 `root`

```python
@property
def root(self) -> 'SGFNode':
    if self._root is None:
        self._root = self.parent.root if self.parent else self
    return self._root
```

计算根节点：如果当前节点有父节点，则递归获取父节点的根；否则自身就是根。结果缓存在 `_root` 中，当父节点改变时被清空。

### 节点深度 `depth`

```python
@property
def depth(self) -> int:
    if self._depth is None:
        moves = self.moves
        if self.is_root:
            self._depth = 0
        else:
            self._depth = self.parent.depth + len(moves)
    return self._depth
```

深度定义为从根到当前节点的“总步数”。注意这里计算的是累计的落子数量（每个节点可能包含多个落子，如 `B[aa]W[bb]` 则算两步）。根节点深度为 0，子节点深度 = 父节点深度 + 当前节点的落子数（`len(self.moves)`）。结果缓存在 `_depth` 中。

### 棋盘大小 `board_size`

```python
@property
def board_size(self) -> Tuple[int, int]:
    size = str(self.root.get_property('SZ', '19'))
    if ':' in size:
        x, y = map(int, size.split(':'))
    else:
        x = int(size)
        y = x
    return x, y
```

从根节点的 `SZ` 属性获取棋盘尺寸。SGF 中 `SZ` 可以是一个数字（正方形）或 `宽:高` 格式（矩形棋盘）。默认为 `"19"`，返回 `(19, 19)`。如果包含冒号，则解析为两个整数。

### 常用根属性 `komi`, `handicap`, `ruleset`

```python
@property
def komi(self) -> float:
    try:
        km = float(self.root.get_property('KM', 6.5))
    except ValueError:
        km = 6.5
    return km

@property
def handicap(self) -> int:
    try:
        return int(self.root.get_property('HA', 0))
    except ValueError:
        return 0

@property
def ruleset(self) -> str:
    return self.root.get_property('RU', 'chinese')
```

从根节点获取贴目、让子数、规则，并处理可能的转换错误。贴目默认 6.5，让子默认 0，规则默认 `"chinese"`。

### 解析落子 `moves`

```python
@property
def moves(self) -> List[Move]:
    if self.moves_cache is None:
        self.moves_cache = [
            Move.from_sgf(move, player=pl, board_size=self.board_size)
            for pl in Move.PLAYERS
            for move in self.get_list_property(pl, [])
        ]
    return self.moves_cache
```

将节点中 `B` 和 `W` 属性（分别表示黑棋和白棋的落子）解析为 `Move` 对象列表。`Move.PLAYERS` 通常是 `"BW"`。每个属性值是一个 SGF 坐标字符串（如 `"pd"`），通过 `Move.from_sgf` 转换为 `Move` 对象，并传入当前棋盘大小。结果缓存在 `moves_cache` 中，属性变更时会被清空。

### 处理放置（Placements）`_expanded_placements`

```python
def _expanded_placements(self, player):
    sgf_pl = player if player is not None else 'E'
    placements = self.get_list_property('A'+sgf_pl, [])
    if not placements:
        return []
    to_be_expanded = [p for p in placements if ':' in p]
    board_size = self.board_size
    if to_be_expanded:
        coords = {
            Move.from_sgf(sgf_coord, player=player, board_size=board_size)
            for sgf_coord in placements
            if ':' not in sgf_coord
        }
        for p in to_be_expanded:
            from_coord, to_coord = [
                Move.from_sgf(c, board_size=board_size)
                for c in p.split(':')[:2]
            ]
            for x in range(from_coord.coords[0], to_coord.coords[0] + 1):
                for y in range(to_coord.coords[1], from_coord.coords[1] + 1):
                    if 0 <= x < board_size[0] and 0 <= y < board_size[1]:
                        coords.add(Move(x,y), player=player)  # 此处可能错误，应该是 Move((x,y), player=player)
        return list(coords)
    else:
        return [Move.from_sgf(sgf_coord, player=player, board_size=board_size)
                for sgf_coord in placements]
```

此方法处理 SGF 中的放置属性：`AB`（黑棋预置）、`AW`（白棋预置）、`AE`（清空点）。参数 `player` 可以是 `"B"`、`"W"` 或 `None`（对应 `AE`）。构造属性名：`'A' + (player if player else 'E')`。

- 获取属性值列表（如 `["dd", "jj:ll"]`）。
- 分离出包含 `:` 的矩形范围（如 `"jj:ll"`）和单个点。
- 如果没有矩形范围，直接转换每个坐标为 `Move` 对象并返回。
- 如果有矩形范围，先转换所有单点存入集合 `coords`，然后对每个矩形范围：
  - 用 `split(':')` 得到起始和结束坐标字符串，分别转换为 `Move` 对象。
  - 遍历从 `from_coord.x` 到 `to_coord.x`、从 `to_coord.y` 到 `from_coord.y`（注意 y 顺序：SGF 中 y 坐标从 0 到 board_size-1 从上到下，矩形 `a:b` 表示从 a 到 b 包括所有点，但方向可能任意。这里用 `range(from_coord.x, to_coord.x+1)` 和 `range(to_coord.y, from_coord.y+1)` 确保覆盖所有点，因为 from_coord 可能坐标较小或较大？实际上 SGF 中矩形通常指定左上和右下，但代码中假定 from_coord 是较小坐标，to_coord 是较大坐标？不，`from_coord` 和 `to_coord` 是从 `p.split(':')` 得到的两个坐标，不保证顺序。但这里使用了 `range(from_coord.x, to_coord.x+1)` 假设 from.x <= to.x，同理 y 假设 from.y >= to.y（因为 y 坐标可能上小下大？）需要谨慎。实际上，SGF 标准中矩形 `aa:dd` 表示所有从 a 到 d 的行和列，无论顺序。因此应该对两个维度分别取最小到最大。当前代码隐含假设了 from 是左上、to 是右下，如果输入是 `dd:aa` 可能出错。但很多 SGF 生成器会按规范提供左上到右下，所以可能可行。
  - 对每个坐标创建 `Move` 对象并加入集合（注意代码中 `coords.add(Move(x,y), player=player)` 语法错误，应为 `Move((x,y), player=player)`，可能是笔误，实际应是 `coords.add(Move(coords=(x,y), player=player))`）。
- 最后返回列表。

### 放置相关属性

#### `placements`

```python
@property
def placements(self) -> List[Move]:
    return [coord for pl in Move.PLAYERS
            for coord in self._expanded_placements(pl)]
```

获取所有黑棋和白棋的放置棋子（`AB` 和 `AW`），展开矩形区域后合并为列表。

#### `clear_placements`

```python
@property
def clear_placements(self)->List[Move]:
    return self._expanded_placements(None)
```

获取所有清空点（`AE`），展开后返回列表。

#### `move_with_placements`

```python
@property
def move_with_placements(self) -> List[Move]:
    return self.placements + self.moves
```

合并放置和落子，返回当前节点所有涉及的棋子变化（通常用于重建棋盘状态）。

### 获取单步 `move`

```python
@property
def move(self) -> Optional[Move]:
    moves = self.moves
    if len(moves) == 1:
        return moves[0]
```

如果节点恰好只有一步落子（不是多步，也不是放置），则返回该步；否则返回 `None`。


### 状态判断属性

- **`is_root`**: 父节点是否为 `None`。
- **`is_pass`**: 没有放置，且仅有一手棋且该棋是 Pass（`Move.is_pass` 为 True）。
- **`empty`**: 没有子节点且没有放置（即没有棋子变化）。注意：`moves` 不计入 `empty`？因为 `empty` 只检查 `children` 和 `placements`，不检查 `moves`，但通常有 `moves` 也会导致有属性，所以 `placements` 可能不包括 `B`/`W`？这里 `placements` 仅指 `AB`/`AW`/`AE`，不包括落子。所以如果节点只有落子（`B` 或 `W`），则 `placements` 为空，但 `children` 可能为空，所以 `empty` 为 True？这似乎不合理。可能意图是：节点如果没有子节点且没有初始放置（即仅作为一步棋的节点，但仍包含落子属性），应视为非空？但根据定义，`empty` 未检查 `moves`，所以包含落子的节点也会被认为是 empty 如果它没有子节点和放置。这可能是一个 bug 或设计疏忽。通常，`empty` 可能用于检测是否是无意义的空节点（如 `;` 无属性），所以它只关心 `placements` 和 `children`，不关心 `moves` 是合理的：因为如果有落子，它就有属性，`placements` 虽然不包含落子，但节点仍非空。然而代码中 `placements` 仅指 `AB`/`AW`/`AE`，所以落子属性不在其中，因此 `empty` 会误判。可能是命名不准确，但实际使用中可能无碍。

### 树遍历方法

#### `nodes_in_tree`

```python
@property
def nodes_in_tree(self) -> List:
    stack = [self]
    nodes = []
    while stack:
        item = stack.pop(0)
        nodes.append(item)
        stack += item.children
    return nodes
```

广度优先遍历（使用队列，`pop(0)` 性能较差但简单），返回当前节点及其所有后代节点列表。

#### `nodes_from_root`

```python
@property
def nodes_from_root(self) -> List:
    nodes = [self]
    n = self
    while not n.is_root:
        n = n.parent
        nodes.append(n)
    return nodes[::-1]
```

沿父链向上直到根，收集路径上的所有节点，然后反转顺序（从根到当前节点）。

### 走子导航 `play`

```python
def play(self, move) -> 'SGFNode':
    for c in self.children:
        if c.move and c.move == move:
            return c
    return self.__class__(parent=self, move=move)
```

根据给定的 `Move` 对象，在子节点中查找是否有相同落子的节点，如果有则返回该子节点；否则创建新节点（以当前节点为父，并设置 `move`），并返回新节点。这用于在棋谱树上沿某步棋前进。

### 玩家相关属性

#### `initial_player`

```python
@property
def initial_player(self):
    root = self.root
    if 'PL' in root.properties:
        return 'B' if self.root.get_property('PL').upper().strip() == 'B' else 'W'
    elif root.children:
        for child in root.children:
            for color in 'BW':
                if color in child.properties:
                    return color
    if 'AB' in self.properties and 'AW' not in self.properties:
        return 'W'
    else:
        return 'B'
```

确定对局的初始玩家（第一手谁下）。规则：
1. 如果根节点有 `PL` 属性，则直接使用该值（`PL` 指定谁先走）。
2. 否则，如果根节点有子节点，检查第一个子节点中包含的 `B` 或 `W` 属性，返回先出现的颜色。
3. 否则，如果当前节点（？应该是根节点？这里用了 `self.properties`，但 `self` 可能是任意节点，通常应在根节点调用。逻辑是：如果根节点有 `AB` 且没有 `AW`，则白先；否则黑先。这是因为有黑棋让子时通常白先。最后默认黑先。

注意：`self.properties` 可能不是根节点的属性，这里可能是个 bug，应该是 `root.properties`。但实际可能期望在根节点调用，所以问题不大。

#### `next_player`

```python
@property
def next_player(self):
    if self.is_root:
        return self.initial_player
    elif 'B' in self.properties:
        return 'W'
    elif 'W' in self.properties:
        return 'B'
    else:
        return self.parent.next_player
```

确定在当前节点之后，下一个该谁下。规则：
- 如果是根节点，则返回初始玩家。
- 如果当前节点有 `B` 属性（表示这一步是黑下的），则下一步是白。
- 如果当前节点有 `W` 属性，则下一步是黑。
- 否则（例如节点只有注释或放置），则继承父节点的 `next_player`。

#### `player`

```python
@property
def player(self):
    if 'B' in self.properties or (
        'AB' in self.properties and 'W' not in self.properties):
        return 'B'
    else:
        return 'W'
```

确定当前节点是谁下的（用于显示）。逻辑：
- 如果节点有 `B` 属性，显然是黑下。
- 或者，如果节点有 `AB` 且没有 `W` 属性，也认为是黑方？这有点奇怪，可能用于设置初始放置时表示黑方放置。否则返回白方。这个属性可能用于标识节点的“颜色”，但定义不够严谨。

### 让子放置 `place_handicap_stones`

```python
def place_handicap_stones(self, n_handicaps, tygen=False):
    board_size_x, board_size_y = self.board_size
    if min(board_size_x, board_size_y) < 3:
        return
    near_x = 3 if board_size_x >= 13 else min(2, board_size_x - 1)
    near_y = 3 if board_size_y >= 13 else min(2, board_size_y - 1)
    far_x = board_size_x - 1 - near_x
    far_y = board_size_y - 1 - near_y
    middle_x = board_size_x // 2
    middle_y = board_size_y // 2

    if n_handicaps > 9 and board_size_x == board_size_y:
        stones_per_row = math.ceil(math.sqrt(n_handicaps))
        spacing = (far_x - near_x) / (stones_per_row - 1)
        if spacing < near_x:
            far_x += 1
            near_x -= 1
            spacing = (far_x - near_x) / (stones_per_row - 1)
        coords = list({math.floor(0.5 + near_x + i * spacing)}
                      for i in range(stones_per_row))
        stones = sorted(
            [(x,y) for x in coords for y in coords],
            key=lambda xy: -((xy[0] - (board_size_x-1) / 2) ** 2  + (xy[1] - (board_size_y-1) / 2) ** 2)
        )
    else:
        stones = [(far_x, far_y), (near_x, near_y), (far_x, near_y), (near_x, far_y)]
        if n_handicaps % 2 == 1:
            stones.append((middle_x, middle_y))
        stones += [(near_x, middle_y), (far_x, middle_y), (middle_x, near_y), (middle_x, far_y)]
    if tygen:
        stones[2], stones[3] = stones[3], stones[2]
    self.set_property(
        'AB', list({
            Move(stone).sgf(board_size=(board_size_x, board_size_y))
            for stone in stones[:n_handicaps]})
    )
```

此方法用于生成让子位置并设置 `AB` 属性。它模拟了围棋让子棋的常见摆法：
- 根据棋盘大小确定星位坐标：`near_x` 和 `near_y` 是离边最近的星位（如 19 路为 3），`far_x` 和 `far_y` 是对角星位。
- 如果让子数大于 9 且棋盘为正方形，则尝试均匀分布多个让子（用于超大让子），通过计算间距得到坐标网格，然后按距离中心从远到近排序（优先放置最远的点）。
- 否则按传统让子顺序：先放四个角星，如果奇数则加天元，然后加边星。
- `tygen` 参数交换第 2 和第 3 个点（可能是为了匹配 Tygem 平台的顺序）。
- 最后将前 `n_handicaps` 个坐标转换为 SGF 字符串，去重后设置 `AB` 属性。

注意：`Move(stone)` 创建 `Move` 对象，默认 player 为 `"B"`，然后调用其 `sgf` 方法生成坐标字符串。去重使用集合，因为可能产生重复坐标。

### 总结

`SGFNode` 类是一个功能完备的 SGF 节点表示，提供了：
- 属性管理（增删改查）
- 树结构操作（父、子、根、深度）
- 棋谱信息提取（棋盘大小、贴目、规则、落子、放置）
- 状态判断和导航
- SGF 字符串生成
- 让子生成等辅助功能

它是整个 SGF 解析库的核心，为上层应用提供了方便的接口来操作和分析围棋棋谱。

# `katrain/core/game_node.py`

该文件定义了 `GameNode` 类，它是 KaTrain 中核心的数据结构之一，继承自 `SGFNode`（来自 `katrain.core.sgf_parser`）。`GameNode` 不仅表示 SGF 树中的一个节点（即一手棋或一个分支点），还封装了与 AI 分析相关的所有数据，以及教学、评论、快捷键等功能。下面逐部分详细解释。

### 1. 导入模块
```python
import base64
import copy
import gzip
import json
import random
from typing import Dict, List, Optional, Tuple

from katrain import i18n
from katrain.core.constants import (
    ADDITIONAL_MOVE_ORDER,
    ANALYSIS_FORMAT_VERSION,
    PRIORITY_DEFAULT,
    PROGRAM_NAME,
    REPORT_DT,
    SGF_INTERNAL_COMMENTS_MARKER,
    SGF_SEPARATOR_MARKER,
    VERSION,
)
from katrain.core.sgf_parser import Move, SGFNode
from katrain.core.utils import evaluation_class, pack_floats, unpack_floats, var_to_grid
from katrain.gui.theme import Theme
```
- **标准库**：`base64`, `gzip`, `json` 用于压缩/解压和编码分析数据；`copy` 用于深拷贝；`random` 用于生成随机阈值；`typing` 用于类型注解。
- **KaTrain 内部模块**：
  - `i18n`：国际化支持。
  - `constants`：各种常量，如走法排序优先级、分析格式版本、默认优先级、程序名、报告间隔、SGF 内部注释标记等。
  - `sgf_parser`：提供 `Move` 和 `SGFNode` 基类。
  - `utils`：工具函数，如评估分类、浮点数打包/解包、将策略数组转换为网格。
  - `theme`：界面主题，用于颜色定义。

### 2. 辅助函数 `analysis_dumps`
```python
def analysis_dumps(analysis):
    analysis = copy.deepcopy(analysis)
    for movedict in analysis["moves"].vlaues():   # 注意：应为 .values()
        if "ownership" in movedict:
            del movedict["ownership"]
    ownership_data = pack_floats(analysis.pop("ownership"))
    policy_data = pack_floats(analysis.pop("policy"))
    main_data = json.dumps(analysis).encode("utf-8")
    return [
        base64.standard_b64encode(gzip.compress(data)).decode("utf-8")
        for data in [ownership_data, policy_data, main_data]
    ]
```
- **作用**：将分析数据压缩并编码为三个 base64 字符串，用于存储到 SGF 的 `KT` 属性中。
- **流程**：
  1. 深拷贝分析数据，避免修改原数据。
  2. 从每个走法字典中删除 `ownership`（所有权图），因为所有权数据已单独打包。
  3. 使用 `pack_floats` 将所有权和策略数据打包为紧凑的浮点字节串。
  4. 将剩余的分析数据（走法信息、根节点信息等）转为 JSON。
  5. 分别对三个数据块进行 gzip 压缩，再 base64 编码，返回字符串列表。

### 3. `GameNode` 类

#### `__init__(self, parent=None, properties=None, move=None)`
初始化节点，继承父类 `SGFNode` 的初始化，并添加大量与 AI 分析、教学相关的属性：
- `auto_undo`：是否自动撤消（教学用）。
- `play_mistake_sound`：是否播放错误音效。
- `ai_thoughts`：AI 思考过程的文本描述。
- `note`：用户添加的注释。
- `move_number`：当前手数。
- `time_used`：本手用时。
- `undo_threshold`：随机阈值，可能用于自动撤消的触发概率。
- `end_state`：终局状态（如终局判断）。
- `shortcuts_to` 和 `shortcut_from`：用于在 SGF 树中建立节点间的快捷链接（例如跳过中间分支）。
- `analysis_from_sgf`：从 SGF 加载的原始分析数据。
- 最后调用 `clear_analysis()` 初始化分析字典。

#### 快捷键相关方法
- **`add_shortcut(self, to_node)`**  
  建立从当前节点到 `to_node` 的快捷链接。它会找到从 `to_node` 到当前节点的路径，并将路径中倒数第二个节点作为中间节点 `via`，然后将 `(to_node, via)` 存入 `shortcuts_to` 列表，并设置 `to_node.shortcut_from = self`。
- **`remove_shortcut(self)`**  
  删除从父节点指向当前节点的快捷链接。若存在 `shortcut_from`，则从父节点的 `shortcuts_to` 中移除当前节点，并将 `shortcut_from` 置为 `None`。

#### `load_analysis(self)`
尝试从 `self.analysis_from_sgf`（即 SGF 的 `KT` 属性）加载分析数据。过程：
- 获取棋盘大小，计算总交叉点数。
- 检查 SGF 中的版本号（`KTV` 属性），若高于当前版本则抛出异常。
- 依次解压并解码三个数据块：所有权、策略、主数据。
- 将主数据 JSON 解析为字典，并用 `unpack_floats` 恢复策略和所有权数组。
- 返回 `True` 表示成功，否则打印错误并返回 `False`。

#### `add_list_property(self, property: str, values: List)`
重写父类方法，用于处理 SGF 属性列表。特别处理两个属性：
- `"KT"`：将值存入 `analysis_from_sgf`。
- `"C"`（注释）：按分隔符 `SGF_SEPARATOR_MARKER` 拆分注释，过滤掉包含内部标记 `SGF_INTERNAL_COMMENTS_MARKER` 的注释行，剩余部分拼接后存入 `self.note`。
- 其他属性调用父类方法处理。

#### `clear_analysis(self)`
重置分析相关数据：
- `analysis_visits_requested`：请求的访问次数（用于引擎分析）。
- `analysis`：字典，包含 `moves`（各走法分析）、`root`（根节点信息）、`ownership`、`policy`、`completed`（是否完成）等键，初始值均为 `None` 或空字典。

#### `sgf_properties(self, save_comments_player=None, save_comments_class=None, eval_thresholds=None, save_analysis=False, save_marks=False)`
生成用于保存到 SGF 的属性字典。继承父类 `sgf_properties` 后，根据参数添加或修改属性：
- 若 `save_analysis` 为 `True` 且分析完整（`analysis_complete`），调用 `analysis_dumps` 生成 `KT` 属性。
- 根据 `points_lost` 和评估阈值，确定是否添加注释。若满足条件，则生成内部注释并加入 `C` 属性。
- 若 `save_marks` 为 `True`，则添加 `SQ`（最佳着法）和 `MA`（标记）属性。
- 若当前节点是根节点，添加 `CA`（编码）、`AP`（程序版本）、`KTV`（分析格式版本）等属性，并插入初始注释。
- 处理快捷键相关属性：`KTSF`（指向来源节点）和 `KTSID`（指向自身，用于标识快捷键目标）。
- 若有用户注释 `note`，插入到注释开头。
- 最终返回属性字典。

#### `order_children(children)` 静态方法
用于排序子节点。排序键为：若 `auto_undo` 为 `None` 则取 `0.5`，否则取 `int(auto_undo)`。这可能是为了将某些子节点（如教学分支）排在前面或后面。

#### `analyze(self, engine, priority=PRIORITY_DEFAULT, visits=None, ponder=False, time_limit=True, refine_move=None, analyze_fast=False, find_alternatives=False, region_of_interest=None, report_every=REPORT_DT)`
向分析引擎请求分析当前节点。调用 `engine.request_analysis`，并传递回调函数 `self.set_analysis` 来处理返回结果。参数控制分析的优先级、访问次数、是否持续思考、时间限制、是否只细化某一步、是否快速分析、是否寻找替代走法、感兴趣区域以及报告间隔。

#### `update_move_analysis(self, move_analysis, move_gtp)`
更新或合并某个走法的分析数据。若 `self.analysis["moves"]` 中尚无此走法，则创建新条目，并设置 `order` 为 `ADDITIONAL_MOVE_ORDER`；若已存在，则取较小的 `order`，若新分析的访问数更多，则更新所有字段，否则仅更新缺失的字段。这样保证了多个分析结果能正确合并。

#### `set_analysis(self, analysis_json: Dict, refine_move: Optional[Move] = None, additional_moves: bool = False, region_of_interest=None, partial_result: bool = False)`
这是引擎回调函数，用于将分析结果写入节点。
- **若 `refine_move` 不为空**（即只细化某一步）：从 `analysis_json` 中提取该步的 PV 和根信息，存入 `moves` 字典。
- **否则**：
  - 若 `additional_moves` 为 `True`，则直接添加所有走法分析（去除 `order` 字段）。
  - 否则（正常查询）：将所有走法分析加入 `moves`，并更新 `ownership`、`policy`。同时，将根信息存入 `analysis['root']`，并回传给父节点（通过 `parent.update_move_analysis`），因为当前节点的根信息对应于父节点下的一个走法。
  - 若既不是细化也不是附加走法，且没有指定感兴趣区域，则标记 `analysis['completed']` 为 `True`（除非是部分结果 `partial_result`）。

#### 属性（property）
- **`ownership` / `policy`**：返回所有权图和策略数组。
- **`analysis_exists`**：根信息是否存在。
- **`analysis_complete`**：分析完成且根信息存在。
- **`root_visits`**：根节点的访问次数。
- **`score`**：根节点的 `scoreLead`，即领先分数。
- **`winrate`**：根节点的 `winrate`，即胜率（0~1）。
- **`points_lost`**：当前走法与父节点最佳走法相比损失的点数。计算方式：`player_sign * (parent_score - score)`。
- **`parent_realized_points_lost`**：当前节点与父节点的父节点相比实际损失的点数（用于评估当前走法对之前局面的影响）。
- **`candidate_moves`**：返回候选走法列表，按 `order` 和损失排序。每个走法字典包含：`pointsLost`, `winrateLost`, `relativePointsLost` 等，以及原始分析数据。若没有分析走法（只有根信息），则根据策略网络生成一个默认走法。
- **`policy_ranking`**：根据策略网络的值，对所有可能走法（包括 pass）按概率从高到低排序，返回 `(概率, Move)` 元组列表。

#### 格式化方法
- **`format_score(self, score=None)`**：将分数格式化为 `B+1.2` 或 `W+1.2` 的字符串。
- **`format_winrate(self, win_rate=None)`**：将胜率格式化为 `B 60%` 或 `W 60%`。
- **`make_pv(self, player, pv, interactive)`**：生成 PV 文本，若 `interactive` 为 `True` 则添加 KaTrain 内部标记以便在 GUI 中显示为可点击链接。
- **`comment(self, sgf=False, teach=False, details=False, interactive=True)`**：生成该节点的详细注释文本，用于显示在界面上或保存到 SGF。内容包括：手数、走法、得分、胜率、损失点数、最佳走法、PV、策略排名、AI 思考、SGF 原始注释等。根据参数控制显示哪些信息。

#### 辅助静态方法
- **`player_sign(player)`**：返回玩家对应的符号：`'B'` -> 1, `'W'` -> -1, `None` -> 0，用于计算得分正负。

### 4. 总结
`GameNode` 是 KaTrain 中连接棋谱（SGF）与 AI 分析的核心桥梁。它扩展了基础的 SGF 节点，增加了：
- **AI 分析数据存储**：所有权、策略、候选走法、根信息等。
- **与引擎交互**：通过 `analyze` 发起分析，通过 `set_analysis` 接收结果。
- **教学功能**：自动撤消标记、错误音效、AI 思考文字。
- **注释与评论生成**：将分析结果转化为人类可读的文本，支持 SGF 保存和 GUI 显示。
- **快捷键**：允许在复杂分支树中建立快捷链接，方便导航。
- **数据持久化**：通过 `sgf_properties` 将分析数据压缩并存储到 SGF 文件中，实现分析结果的保存与加载。

整个类设计严谨，兼顾了围棋 AI 分析的专业需求与教学工具的易用性。

# `katrain/core/engine.py`

该文件定义了与 KataGo 围棋 AI 引擎交互的核心模块，负责启动/管理引擎进程、发送分析请求、接收并分发结果、处理错误和恢复。主要包含两个类：`BaseEngine`（基类）和 `KataGoEngine`（具体实现）。

### 1. 导入模块
```python
import copy
import json
import os
import platform
import queue
import shlex
import subprocess
import threading
import time
import traceback
from typing import Callable, Dict, List, Optional

from kivy.utils import platform as kivy_platform

from katrain.core.constants import (
    OUTPUT_DEBUG, OUTPUT_ERROR, OUTPUT_EXTRA_DEBUG, OUTPUT_KATAGO_STDERR,
    DATA_FOLDER, KATAGO_EXCEPTION, PONDERING_REPORT_DT,
)
from katrain.core.game_node import GameNode
from katrain.core.lang import i18n
from katrain.core.sgf_parser import Move
from katrain.core.utils import find_package_resource, json_truncate_arrays
```
- **标准库**：处理进程、线程、队列、JSON、路径等。
- **Kivy 平台检测**：用于判断操作系统（win/linux/ios等），以便找到正确的 KataGo 可执行文件。
- **内部模块**：常量、节点类、国际化、SGF 移动对象、工具函数（查找资源、截断 JSON 数组）。

### 2. `BaseEngine` 类

#### 类变量 `RULESETS_ABBR` 和 `RULESETS`
```python
RULESETS_ABBR = [
    ("jp", "japanese"), ("cn", "chinese"), ("ko", "korean"),
    ("aga", "aga"), ("tt", "tromp-taylor"),
    ("nz", "new zealand"), ("stone_scoring", "stone_scoring"),
]
RULESETS = {fromkey: name for abbr, name in RULESETS_ABBR for fromkey in [abbr, name]}
```
- 提供规则集的缩写与全称的映射，用于 KataGo 的 `rules` 参数。

#### `__init__(self, katrain, config)`
保存主程序实例和配置字典。

#### `get_rules(ruleset: str)` 静态方法
- 若 `ruleset` 以 `{` 开头，尝试解析为 JSON 字典（用于自定义规则）。
- 否则，在 `RULESETS` 中查找，若找不到则默认返回 `"chinese"`。

#### `advance_showing_game(self)`、`status(self)` 占位方法
供子类重写，用于更新显示或获取状态。

#### `get_engine_path(self, exe)`
根据配置的 `exe` 名称查找 KataGo 可执行文件的完整路径：
- 若未指定，根据平台选择默认路径（Windows 为 `katrain/KataGo/katago.exe`，Linux 为同名，macOS 则先尝试包内路径，若找不到则假设 `katago` 在 PATH 中）。
- 如果路径以 `katrain` 开头，使用 `find_package_resource` 在包内查找。
- 如果路径是相对路径或只有文件名，则在 PATH 和 `/opt/homebrew/bin/` 中搜索。
- 若找不到，通过 `on_error` 报告错误。

#### `on_error(self, message, code, allow_popup)`
默认打印错误，期望被子类重写以显示给用户。

### 3. `KataGoEngine` 类（继承 `BaseEngine`）

这是实际与 KataGo 进程交互的引擎类，负责管理查询队列、线程、进程生命周期。

#### 类常量 `PONDER_KEY = "_kt_continuous"`
用于标识持续思考（pondering）的查询。

#### `__init__(self, katrain, config)`
- 保存配置，初始化属性：
  - `allow_recovery`：是否允许弹出恢复对话框。
  - `queries`：字典，存储正在进行的查询（ID -> (callback, error_callback, start_time, next_move, node)）。
  - `ponder_query`：当前持续思考的查询对象。
  - `query_counter`：自增ID。
  - `katago_process`：子进程对象。
  - `base_priority`：基础优先级，每局游戏递增，确保新游戏的查询优先级更高。
  - `override_settings`：强制传递给 KataGo 的配置，如 `{"reportAnalysisWinratesAs": "BLACK"}`。
  - 线程相关：`analysis_thread`（读 stdout）、`stderr_thread`（读 stderr）、`write_stdin_thread`（写 stdin），以及 `write_queue`（请求队列）、`thread_lock`（线程锁）。
- **构建启动命令**：
  - 如果配置中有 `altcommand`，直接使用该命令（shell=True）。
  - 否则，从配置中获取 `katago`（可执行文件路径）、`model`（模型文件）、`config`（KataGo 配置文件），并通过 `get_engine_path` 解析路径。
  - 如果配置了 `humanlike_model`（人性化模型文件），则构造包含 `-human-model` 参数的命令；否则构造普通命令。均加上 `-override-config "homeDataDir={DATA_FOLDER}"` 以设置数据目录。
  - 最后调用 `self.start()` 启动进程。

#### `on_error(self, message, code=None, allow_popup=True)`
重写基类方法，通过 `katrain.log` 记录错误日志，并根据 `allow_recovery` 决定是否弹出恢复对话框（调用 `katrain` 对象的回调）。

#### `start(self)`
- 初始化新队列，创建子进程（`subprocess.Popen`），设置 stdin/stdout/stderr 为管道。
- 启动三个守护线程：
  - `analysis_thread`：读取 KataGo 的 stdout（分析结果）。
  - `stderr_thread`：读取 stderr（日志和错误信息）。
  - `write_stdin_thread`：从队列中取出请求并写入 KataGo 的 stdin。

#### `on_new_game(self)`
- 递增 `base_priority`，使新游戏的查询优先级更高。
- 清空队列，终止所有正在进行的查询（调用 `terminate_queries`），重置 `ponder_query` 和 `queries`。

#### `terminate_queries(self, only_for_node=None, lock=True)`
- 终止所有或特定节点的查询。
- 内部循环：遍历 `queries`，若匹配则调用 `terminate_query`。

#### `stop_pondering(self)`
终止当前的持续思考查询。

#### `terminate_query(self, query_id, ignore_further_results=True)`
- 向 KataGo 发送 `{"action": "terminate", "terminateId": query_id}` 以停止指定查询。
- 若 `ignore_further_results` 为真，立即从 `queries` 中移除该 ID，防止后续结果被处理。

#### `restart(self)`
终止当前进程并重新启动。

#### `check_alive(self, os_error='', exception_if_dead=False, maybe_open_recovery=False)`
- 检查进程是否仍在运行（`poll() is None`）。
- 若已死亡且 `exception_if_dead` 为真，根据退出码生成错误消息并调用 `on_error`（可能弹出恢复对话框）。
- 返回 `True` 表示存活，`False` 表示已死。

#### `wait_to_finish(self)`
等待所有查询完成（`queries` 为空且队列空）。

#### `shutdown(self, finish=False)`
- 若 `finish` 为真，先等待所有查询完成。
- 终止 KataGo 进程。
- 若 `finish` 不为 `None`，等待三个线程结束（`join`）。

#### `is_idle(self)`、`queries_remaining(self)`
检查引擎空闲状态。

#### 线程方法

##### `_read_stderr_thread(self)`
- 循环读取 KataGo 的 stderr 行。
- 若行中包含 `"Uncaught exception"` 或 `"what()"`（C++ 异常标志），报告错误并退出。
- 否则将行内容通过 `katrain.log` 记录为 `OUTPUT_KATAGO_STDERR`。
- 若读取到空行，检查进程是否存活；若已死则退出。

##### `_analysis_read_thread(self)`
- 循环读取 KataGo 的 stdout 行（JSON 格式）。
- 忽略空行，解析 JSON。
- 检查 ID 是否在 `queries` 中，若不在则丢弃（可能因新游戏导致）。
- 处理错误、警告、终止确认、正常结果。
  - 正常结果（可能包含 `isDuringSearch` 表示部分结果）：
    - 若非部分结果，从 `queries` 中删除。
    - 记录日志，调用回调函数 `callback(analysis, partial_result)`。
- 捕获异常，记录错误，并打印 traceback。

##### `_write_stdin_thread(self)`
- 循环从 `write_queue` 获取请求（`get` 阻塞，超时 0.1 秒以便检查进程是否死亡）。
- 若请求无 `id`，分配新 ID（`QUERY:<计数器>`）。
- 处理持续思考（pondering）：
  - 从请求中弹出 `self.PONDER_KEY`。
  - 若为 `True`，比较当前 `ponder_query` 与请求的差异（排除 `id`、`maxVisits`、`reportDuringSearchEvery`）。若有差异，则停止当前 pondering，设置新请求的 `maxVisits` 为极大值（1000万）和报告间隔为 `PONDERING_REPORT_DT`，并更新 `ponder_query`；若完全相同，则跳过发送（避免重复）。
- 若非终止请求，将 `(callback, error_callback, start_time, next_move, node)` 存入 `queries[query_id]`。
- 将 JSON 请求写入 KataGo 的 stdin，并 flush。

#### `send_query(self, query, callback, error_callback, next_move=None, node=None)`
将请求放入 `write_queue`，供写入线程处理。

#### `request_analysis(self, analysis_node: GameNode, callback: Callable, error_callback: Optional[Callable] = None, ...)`
这是外部调用的主要接口，用于请求对某个游戏节点进行分析。
- **参数**：分析节点、回调函数、错误回调、访问次数、是否快速分析、时间限制、寻找替代走法、感兴趣区域、优先级、持续思考标志、是否包含所有权、下一步走法、额外设置、是否包含策略网络、报告间隔等。
- **流程**：
  1. 从节点向上获取所有祖先节点，提取历史走法（`moves`）、初始棋子（`initialStones`，例如置石），并检查是否有 `AE`（清除棋子）命令，若有则跳过分析。
  2. 若指定 `next_move`，将其附加到走法列表。
  3. 确定 `ownership` 选项（通常启用，但若指定了下一步走法则禁用，因为所有权仅对当前局面有意义）。
  4. 确定访问次数：若 `analyze_fast` 且配置了 `fast_visits`，则使用该值；否则使用配置的 `max_visits`。
  5. 处理 `find_alternatives` 或 `region_of_interest`：生成 `avoidMoves` 列表，告诉 KataGo 避免探索某些走法（用于缩小搜索范围或强制探索非主流走法）。
  6. 构建设置：合并 `override_settings` 和用户传入的 `extra_settings`，并添加 `wideRootNoise` 和 `maxTime`（若启用时间限制）。
  7. 组装 `query` 字典，包含规则、优先级、要分析的回合（当前局面）、最大访问次数、贴目、棋盘大小、所有权选项、策略选项、初始棋子、当前走法、覆盖设置，以及可选的报告间隔和避免走法列表。
  8. 调用 `send_query` 发送请求。
  9. 更新节点的 `analysis_visits_requested`，记录请求的访问次数（用于界面显示）。

### 4. 核心设计思想

- **异步非阻塞**：所有与 KataGo 的通信均通过线程和队列，主线程不会被阻塞。
- **查询管理**：每个查询有唯一 ID，回调函数与节点绑定，结果返回时调用对应回调。
- **持续思考（Pondering）**：通过特殊的 `_kt_continuous` 标志和 `ponder_query` 实现。当没有活跃查询时，引擎可以持续思考当前局面，但只保留一个 pondering 查询，并在局面变化时自动切换。
- **错误恢复**：监控进程异常退出，通过弹出对话框让用户选择重启或使用备用引擎。
- **灵活性**：支持自定义命令、人性化模型、感兴趣区域、避免走法等多种 KataGo 高级特性。

### 5. 注意事项与潜在问题

- 代码中有一些拼写错误，例如 `shudown` 应为 `shutdown`，`vlaues`（在 `analysis_dumps` 函数中）应为 `values`，但这些不影响核心逻辑（用户提供的文件是 `game_node.py`，但问题中我们分析的是 `engine.py`，没有发现 `analysis_dumps` 错误）。
- 线程安全：使用 `thread_lock` 保护对共享字典 `queries` 的访问，但 `write_queue` 本身是线程安全的队列。
- 资源清理：`shutdown` 方法会终止进程并等待线程结束，确保程序退出时不会遗留僵尸线程。

### 6. 总结

`engine.py` 是 KaTrain 与 KataGo 引擎之间的桥梁，封装了进程管理、请求队列、结果分发、错误处理等复杂逻辑，为上层（如游戏分析、教学）提供了简洁的 `request_analysis` 接口。通过多线程和队列，实现了高效的异步分析，并支持持续思考、区域限制等高级功能。

# `katrain/core/game.py`

`game.py` 是 KaTrain 项目中核心的游戏逻辑模块，它定义了 `Game` 类（继承自 `BaseGame`），负责管理一局围棋游戏的完整生命周期，包括：

- 棋盘状态的维护（通过继承的 `BaseGame` 实现）
- 与 KataGo 引擎的交互（分析、自对弈、额外分析等）
- SGF 文件的读写与节点树管理
- 教学功能（如自动撤销、插入模式）
- 用户界面状态的联动（通过 `katrain` 对象与 Kivy GUI 通信）

以下逐部分详解代码。

## 1. 导入模块与常量

```python
import copy, math, os, re, threading
from datetime import datetime
from typing import Dict, List, Optional, Union

from kivy.clock import Clock

from katrain.core.constants import (
    OUTPUT_DEBUG, OUTPUT_EXTRA_DEBUG, OUTPUT_INFO,
    PLAYER_AI, PLAYER_HUMAN, PROGRAM_NAME, SGF_INTERNAL_COMMENTS_MARKER,
    STATUS_ANALYSIS, STATUS_ERROR, STATUS_INFO, STATUS_TEACHING,
    PRIORITY_GAME_ANALYSIS, PRIORITY_EXTRA_ANALYSIS, PRIORITY_SWEEP,
    PRIORITY_ALTERNATIVES, PRIORITY_EQUALIZE, PRIORITY_DEFAULT,
)
from katrain.core.engine import KataGoEngine
from katrain.core.game_node import GameNode
from katrain.core.lang import i18n, rank_label
from katrain.core.sgf_parser import SGF, Move
from katrain.core.utils import var_to_grid, weighted_selection_without_replacement
```

- 导入了大量 KaTrain 内部模块，包括常量、引擎、节点类、国际化、SGF 解析工具等。
- `threading` 用于异步分析，`kivy.clock` 用于 GUI 线程调度。

---

## 2. 自定义异常

```python
class IllegalMoveException(Exception):
    pass
```

简单的异常类，用于标识不合法的落子（如占位、禁着点、规则不允许等）。

## 3. `KaTrainSGF` 类

```python
class KaTrainSGF(SGF):
    _NODE_CLASS = GameNode
```

继承自 `sgf_parser.SGF`，仅将节点类替换为 `GameNode`，使得解析出的 SGF 树中的节点都是 `GameNode` 类型，便于存储分析数据。

## 4. `BaseGame` 类

`BaseGame` 是一个抽象基类，实现了围棋的基本规则（棋子分组、提子、禁着等）。它不涉及分析或 GUI 交互，仅维护纯游戏状态。

### 4.1 初始化

```python
def __init__(self, katrain, move_tree: GameNode = None, game_properties: Optional[Dict] = None,
             sgf_filename=None, bypass_config=False):
    self.katrain = katrain
    self._lock = threading.Lock()
    self.game_id = datetime.strftime(datetime.now(), "%Y-%m-%d %H %M %S")
    self.sgf_filename = sgf_filename
    self.insert_mode = False
    self.external_game = False
    ...
```

- `katrain`：主应用对象，用于访问配置、GUI 等。
- `_lock`：线程锁，确保多线程下棋盘状态的一致。
- `game_id`：用于生成默认文件名。
- `insert_mode`、`external_game` 等标志用于教学和判断是否由 KaTrain 生成。

#### 加载已有 SGF 树

```python
if move_tree:
    self.root = move_tree
    self.external_game = PROGRAM_NAME not in self.root.get_property("AP", "")
    # 处理 handicap 特殊情况
    ...
else:
    # 创建新游戏，从配置读取大小、贴目、规则等
    ...
```

- 如果提供了 `move_tree`（即已有的 SGF 节点树），则直接使用，否则根据配置创建一个新根节点。
- 处理 handicap：如果 SGF 中声明了 handicap 但未放置棋子，则调用 `root.place_handicap_stones(handicap)` 放置。
- 若缺少规则属性，则从当前配置继承。

#### 恢复快捷键

```python
shortcut_id_to_node = {node.get_property("KTSID", None): node for node in self.root.nodes_in_tree}
for node in self.root.nodes_in_tree:
    shortcut_id = node.get_property("KTSF", None)
    if shortcut_id and shortcut_id in shortcut_id_to_node:
        shortcut_id_to_node[shortcut_id].add_shortcut(node)
```

- 快捷键（shortcut）是 KaTrain 自定义的节点间跳转关系，用于快速回退到之前的分支。这里根据 SGF 中保存的 `KTSF` / `KTSID` 属性恢复。

### 4.2 棋盘状态维护方法

- `_init_state()`：初始化空棋盘、链列表、提子列表。
- `_calculate_groups()`：从当前节点开始，回溯所有历史着法，重新计算所有棋链（groups）。这个方法在每次切换节点时调用，以确保棋盘状态与当前节点匹配。
- `_validate_move_and_update_chains(move, ignore_ko)`：核心规则检查。给定一步棋，更新棋链、处理提子、检查禁着（包括打劫、自杀等）。若非法则抛出 `IllegalMoveException`。
- `play(move, ignore_ko)`：公开的落子接口，调用上述方法，并创建新节点，最后返回新节点。

### 4.3 移动树导航

- `sync_branch(moves)`：从根开始依次执行一系列着法，返回最后一个节点（用于快速重建某条分支）。
- `set_current_node(node)`：设置当前节点并重新计算棋盘。
- `undo(n_times, stop_on_mistake)`：回退若干步，支持回退到主分支或分支起点。
- `redo(n_times, stop_on_mistake)`：前进若干步。

### 4.4 属性访问

- `komi`, `board_size`, `stones`, `prisoner_count`, `rules` 等。
- `end_result`：判断是否连续两次 pass，若是则返回对局结果。
- `manual_score`：尝试利用神经网络所有权预测估算终局目数（用于日本规则）。

### 4.5 SGF 相关

- `update_root_properties()`：将玩家信息（名称、类型、段位）写入根节点的 `PB` / `PW` 等属性。
- `generate_filename()`：生成默认保存文件名。
- `write_sgf(filename, trainer_config)`：将整棵树输出为 SGF 字符串，保存到文件。根据配置决定是否保存分析数据、标记等。

## 5. `Game` 类（继承 `BaseGame`）

`Game` 在 `BaseGame` 基础上增加了与 KataGo 引擎的交互、教学功能、插入模式、额外分析等高级特性。

### 5.1 初始化

```python
def __init__(self, katrain, engine: Union[Dict, KataGoEngine], move_tree: GameNode = None,
             analyze_fast=False, game_properties: Optional[Dict] = None, sgf_filename=None):
    super().__init__(katrain=katrain, move_tree=move_tree, game_properties=game_properties,
                     sgf_filename=sgf_filename)
    if not isinstance(engine, Dict):
        engine = {"B": engine, "W": engine}
    self.engines = engine
    self.insert_mode = False
    self.insert_after = None
    self.region_of_interest = None
    threading.Thread(target=lambda: self.analyze_all_nodes(analyze_fast=analyze_fast, even_if_present=False),
                     daemon=True).start()
```

- `engine` 可以是单个引擎或黑白分别指定的引擎字典。通常黑白使用同一引擎。
- `engines` 存储黑白方对应的引擎对象。
- `insert_mode` / `insert_after`：用于教学插入模式。
- `region_of_interest`：限定分析区域（用于局部训练）。
- 在后台线程启动 `analyze_all_nodes`，对树中所有尚未分析的节点发起分析请求（若 SGF 中已存有分析数据且可加载，则直接加载而不重新分析）。

### 5.2 分析管理

#### `analyze_all_nodes`

```python
def analyze_all_nodes(self, priority=PRIORITY_GAME_ANALYSIS, analyze_fast=False, even_if_present=True):
    for node in self.root.nodes_in_tree:
        if even_if_present or not node.analysis_from_sgf or not node.load_analysis():
            node.clear_analysis()
            node.analyze(self.engines[node.next_player], priority=priority, analyze_fast=analyze_fast)
```

遍历所有节点，若允许强制分析（`even_if_present`）或 SGF 中无有效分析数据，则清除旧分析并发起新的分析请求。`analyze` 方法定义在 `GameNode` 中，最终会调用引擎的 `request_analysis`。

#### `reset_current_analysis`

```python
def reset_current_analysis(self):
    cn = self.current_node
    engine = self.engines[cn.next_player]
    engine.terminate_queries(cn)
    cn.clear_analysis()
    cn.analyze(engine)
```

终止当前节点正在进行的分析查询，清除已有结果，重新发起分析。

#### `set_region_of_interest`

```python
def set_region_of_interest(self, region_of_interest):
    x1, x2, y1, y2 = region_of_interest
    xmin, xmax = min(x1, x2), max(x1, x2)
    ymin, ymax = min(y1, y2), max(y1, y2)
    szx, szy = self.board_size
    if not (xmin == xmax and ymin == ymax) and not (xmax - xmin + 1 >= szx and ymax - ymin + 1 >= szy):
        self.region_of_interest = [xmin, xmax, ymin, ymax]
    else:
        self.region_of_interest = None
    self.katrain.controls.set_status("", OUTPUT_INFO)
```

根据用户框选的矩形区域设置局部兴趣区，后续分析将只关注该区域（用于训练局部死活等）。

### 5.3 额外分析（`analyze_extra`）

```python
def analyze_extra(self, mode, **kwargs):
```

该方法处理用户从界面触发的各种额外分析模式：

- **stop**：停止所有分析（终止引擎查询、停止 ponder）。
- **ponder**：启动后台思考（ponder），不设时间限制。
- **extra**：增加当前节点的访问量（`visits`），进行更深入分析。
- **game**：对整个对局的所有节点进行重新分析，可选择只分析失误（`mistakes_only`）或限定手数范围（`move_range`）。
- **sweep**：对所有合法空点进行快速分析（用于探索）。
- **equalize**：对当前节点的候选着法进行等访问量分析，确保每个候选点都有相同的 `visits`。
- **alternative**：启动备选着法分析（快速扫描候选点）。
- **local**：与 `sweep` 类似，但用于局部区域。

根据模式不同，调用 `node.analyze` 时传入不同的参数（`visits`、`refine_move`、`find_alternatives`、`priority` 等）。

### 5.4 教学与插入模式

#### `set_insert_mode`

```python
def set_insert_mode(self, mode):
    if mode == "toggle":
        mode = not self.insert_mode
    if mode == self.insert_mode:
        return
    self.insert_mode = mode
    if mode:
        children = self.current_node.ordered_children
        if not children:
            self.insert_mode = False
        else:
            self.insert_after = self.current_node.ordered_children[0]
            self.katrain.controls.set_status(i18n._("starting insert mode"), STATUS_INFO)
    else:
        # 退出插入模式：将当前节点之后的路径复制到插入点
        ...
        self._calculate_groups()
        self.analyze_all_nodes(analyze_fast=True, even_if_present=False)
    self.katrain.controls.move_tree.insert_node = self.insert_after if self.insert_mode else None
    self.katrain.controls.move_tree.redraw()
    self.katrain.update_state(redraw_board=True)
```

插入模式允许用户在现有分支的某处插入新的变化。当进入插入模式时，记录当前节点第一个子节点作为插入目标 `insert_after`；退出时，将当前节点到终点的所有着法尝试逐一插入到 `insert_after` 之后（若合法则保留，否则停止）。最后重新分析受影响的节点。

#### 导航方法重写

```python
def undo(self, n_times=1, stop_on_mistake=None):
    if self.insert_mode:
        # 插入模式下，撤销操作变为删除当前节点
        ...
        return
    super().undo(n_times=n_times, stop_on_mistake=stop_on_mistake)

def redo(self, n_times=1, stop_on_mistake=None):
    if self.insert_mode:
        return
    super().redo(n_times=n_times, stop_on_mistake=stop_on_mistake)
```

在插入模式下，`undo` 行为被重定义为删除当前节点，而 `redo` 被禁用，防止在编辑过程中意外前进。

### 5.5 自对弈（`selfplay`）

```python
def selfplay(self, until_move, target_b_advantage=None):
```

实现 AI 自我对弈功能，常用于生成训练数据或设定特定局面。

- `until_move`：指定要下到第几手（或 `"end"` 下到终局）。
- `target_b_advantage`：可选的目标黑方优势，用于引导自对弈走向特定胜负差距。

流程：
1. 定义内部函数 `request_analysis_for_node` 和 `analyze_and_play`。
2. 对当前节点请求分析，得到候选着法列表。
3. 如果指定了 `target_b_advantage`，则根据目标分数和当前分数的差距，对候选着法进行加权随机选择（使用高斯权重），使得每一步尽量接近目标分数。
4. 否则直接选最高概率着法。
5. 创建新节点，设置 `ai_thoughts` 注释（用于记录选择理由），并建立快捷键（`add_shortcut`）以便回退。
6. 对新节点再次发起分析，循环直到达到指定手数或终局。

### 5.6 教学撤销（`analyze_undo`）

```python
def analyze_undo(self, node):
    train_config = self.katrain.config("trainer")
    move = node.move
    if node != self.current_node or node.auto_undo is not None or not node.analysis_complete or not move:
        return
    points_lost = node.points_lost
    thresholds = train_config["eval_thresholds"]
    num_undo_prompts = train_config["num_undo_prompts"]
    i = 0
    while i < len(thresholds) and points_lost < thresholds[i]:
        i += 1
    num_undos = num_undo_prompts[i] if i < len(num_undo_prompts) else 0
    if num_undos == 0:
        undo = False
    elif num_undos < 1:  # 概率
        undo = int(node.undo_threshold < num_undos) and len(node.parent.children) == 1
    else:
        undo = len(node.parent.children) <= num_undos
    node.auto_undo = undo
    if undo:
        self.undo(1)
        self.katrain.controls.set_status(
            i18n._("teaching undo message").format(move=move.gtp(), points_lost=points_lost), STATUS_TEACHING
        )
        self.katrain.update_state()
```

这是教学功能的核心：根据玩家失误的大小（`points_lost`）决定是否自动撤销。配置文件中定义了多个阈值和对应的撤销次数/概率。若满足条件，则执行一次撤销并在界面上显示提示。

## 6. 总结

`game.py` 中的 `Game` 类是一个高度集成的围棋游戏控制器，它：

- 继承了 `BaseGame` 的所有基础规则和树管理能力。
- 通过 `engines` 与 KataGo 引擎通信，为每个节点请求分析，存储分析结果。
- 提供了多种分析模式（全局重分析、局部扫点、备选着法、自对弈等），满足不同训练和研究需求。
- 实现了教学插入模式，允许用户在任意分支中插入新变化。
- 集成了自动撤销机制，根据失误大小智能提示或回退。
- 负责 SGF 文件的读写、节点属性的维护（如快捷键、分析数据压缩存储）。
- 与 Kivy GUI 紧密协作（通过 `katrain` 对象更新状态、设置状态栏等）。

整个模块设计注重线程安全（使用 `_lock`），并在后台线程中进行耗时分析，避免阻塞 UI。其代码结构清晰，职责划分合理，是 KaTrain 作为围棋 AI 教学工具的核心。

# `katrain/core/ai.py`

`ai.py` 是 KaTrain 中实现 AI 落子策略的核心模块。它采用**策略模式**，定义了一系列 AI 行为（从纯引擎最强着法到模拟人类不同水平/风格的着法），并通过注册机制动态创建策略对象。以下是对代码的详细解析。

## 1. 导入与常量
- 从 `katrain.core.constants` 导入大量 AI 模式名称（如 `AI_DEFAULT`、`AI_HANDICAP`、`AI_SCORELOSS` 等）和优先级、输出级别等常量。
- 从 `katrain.core.game` 导入 `Game`、`GameNode`、`Move`。
- 从 `katrain.core.utils` 导入工具函数：`var_to_grid`（一维数组转二维网格）、`weighted_selection_without_replacement`（加权不放回随机选择）、`evaluation_class`（按失分阈值分类）。

## 2. 策略注册机制
```python
STRATEGY_REGISTRY = {}

def register_strategy(strategy_name):
    def decorator(strategy_class):
        STRATEGY_REGISTRY[strategy_name] = strategy_class
        return strategy_class
    return decorator
```
装饰器工厂，将策略类与字符串名称关联。例如 `@register_strategy(AI_DEFAULT)` 将 `DefaultStrategy` 注册到 `STRATEGY_REGISTRY`，供后续动态创建。

## 3. 插值与棋力估算辅助函数
- **`interp_ix`**、**`interp1d`**、**`interp2d`**：实现一维/二维线性插值，用于根据设置参数估算棋力（ELO）或段位。
- **`ai_rank_estimation`**：根据策略类型和设置计算对应的棋力等级（段位或 ELO），用于界面显示或校准。

## 4. 对局报告 `game_report`
```python
def game_report(game, thresholds, depth_filter=None):
```
生成对局统计报告：计算每个玩家的准确率、复杂度、平均失分、AI 前 N 命中率等，返回统计字典、直方图和失分列表，常用于教学分析。

## 5. 通用权重生成工具
- **`fmt_moves`**：格式化输出 (概率, 着法) 列表。
- **`policy_weighted_move`**：根据策略网络概率进行加权随机选择，`weaken_fac` 控制分布平滑度。
- **`generate_influence_territory_weights`**：生成基于**影响力**（靠近边线）或**领地**（靠近中心）的权重，与策略概率相乘。
- **`generate_local_tenuki_weights`**：生成基于**局部**（靠近上一手）或**脱先**（远离上一手）的权重，使用高斯函数。

## 6. 抽象基类 `AIStrategy`
所有策略的基类，定义通用接口和方法：
- `__init__(self, game, ai_settings)`：保存游戏对象、当前节点、设置。
- `@abstractmethod generate_move(self) -> Tuple[Move, str]`：子类必须实现，返回着法和解释。
- `request_analysis(self, extra_settings)`：向引擎请求带特殊设置的分析，并等待结果。
- `wait_for_analysis(self)`：等待当前节点的常规分析完成。
- `should_play_top_move(...)`：检查是否应无视策略直接下策略网络的首选着法（条件如 pass 在前五、首着概率超阈值、前两着概率和超阈值）。

## 7. 具体策略类（按注册顺序）

### 7.1 `DefaultStrategy` (AI_DEFAULT)
- **逻辑**：等待分析完成，取候选着法列表的第一个（引擎最佳着法），若无则下 pass。
- **用途**：最强引擎模式。

### 7.2 `HandicapStrategy` (AI_HANDICAP)
- **逻辑**：根据 `pda`（playoutDoublingAdvantage）或自动计算（让子数+贴目）请求带 PDA 的额外分析，然后取最佳着法。
- **用途**：让子局调整引擎强度。

### 7.3 `AntimirrorStrategy` (AI_ANTIMIRROR)
- **逻辑**：请求带 `antiMirror=True` 的分析，防止引擎模仿对手（对抗模仿棋），取最佳着法。

### 7.4 `JigoStrategy` (AI_JIGO)
- **逻辑**：从候选着法中选择使最终分数最接近目标分数（如半目胜）的着法。
- **用途**：追求特定优势的着法。

### 7.5 `ScoreLossStrategy` (AI_SCORELOSS)
- **逻辑**：根据失分计算权重 `exp(-c * max(0, points_lost))`（`c` 由 `strength` 控制），然后加权随机选择。
- **用途**：模拟弱一些的 AI，避免大失误但允许小失误。

### 7.6 `OwnershipBaseStrategy`（抽象基类）
为基于所有权（ownership）的策略提供基础，包含：
- `settledness`：计算该着法后对己方/对方棋子的安定程度（所有权绝对值之和）。
- `is_attachment`：判断是否贴对方棋子。
- `is_tenuki`：判断是否脱先。
- `get_moves_with_settledness`：筛选候选着法，计算综合得分（失分 + 贴/脱先惩罚 - 安定奖励），返回排序列表。

#### 7.6.1 `SimpleOwnershipStrategy` (AI_SIMPLE_OWNERSHIP)
- 使用 `get_moves_with_settledness` 取第一个着法，偏好安定自己或破坏对方安定的点。

#### 7.6.2 `SettleStonesStrategy` (AI_SETTLE_STONES)
- 重写 `settledness`，计算现有棋子的所有权绝对值之和，强调巩固已有棋子。

### 7.7 `PolicyStrategy` (AI_POLICY)
- **逻辑**：若 pass 在策略网络前五，下策略首着；开局阶段（`depth <= opening_moves`）转用加权策略，否则直接下策略首着。
- **混合策略**，兼顾开局变化与后期确定性。

### 7.8 `WeightedStrategy` (AI_WEIGHTED)
- **逻辑**：用 `weaken_fac` 调整策略概率分布（概率的 1/weaken_fac 次方），排除低于 `lower_bound` 的着法，加权随机选择，同时检查覆盖条件。
- **用途**：模拟不同力度的随机性，可调弱。

### 7.9 `PickBasedStrategy`（抽象基类）
为基于挑选一定数量候选的策略提供通用流程：
- `get_n_moves`：根据 `pick_frac` 和 `pick_n` 计算要挑选的着法数量。
- `generate_weighted_coords`：生成带权重的坐标（子类可重写）。
- `handle_endgame`：判断是否进入官子，若是则改用等权并增加数量。
- `select_from_weighted_coords`：加权不放回选 `n_moves` 个，取其中策略概率最高的着法，并与 pass 概率比较（若 pass 更高则改下策略首着）。
- `generate_move` 模板方法执行上述流程。

#### 7.9.1 `PickStrategy` (AI_PICK)
- 默认等权生成坐标，从所有合法点中随机选一定数量，再从中选策略概率最高的着法。

#### 7.9.2 `RankStrategy` (AI_RANK)
- 重写 `get_n_moves`，用复杂公式根据段位（`kyu_rank`）计算挑选数量。
- 重写 `should_play_top_move`，根据盘面填充程度动态调整覆盖阈值。

#### 7.9.3 `InfluenceStrategy` (AI_INFLUENCE)
- 重写 `generate_weighted_coords`，调用 `generate_influence_territory_weights` 生成靠近边线的权重。

#### 7.9.4 `TerritoryStrategy` (AI_TERRITORY)
- 重写 `generate_weighted_coords`，调用 `generate_influence_territory_weights` 生成靠近中心的权重。

#### 7.9.5 `LocalStrategy` (AI_LOCAL)
- 若无上一手则回退到加权策略；重写 `generate_weighted_coords`，调用 `generate_local_tenuki_weights` 生成靠近上一手的权重。

#### 7.9.6 `TenukiStrategy` (AI_TENUKI)
- 若无上一手则回退到加权策略；重写 `generate_weighted_coords`，调用 `generate_local_tenuki_weights` 生成远离上一手的权重。

### 7.10 `HumanStyleStrategy` (AI_HUMAN / AI_PRO)
- 注册两个名称，逻辑相同。
- **逻辑**：构造 `humanProfile` 字符串（如 `"rank_1k"` 或 `"proyear_2020"`），向引擎请求包含 `humanPolicy` 的分析（需人类风格模型）。若分析成功，从 `humanPolicy` 概率中加权随机选一手；若失败，回退到策略网络首着。
- **用途**：模拟人类不同水平的着法。

## 8. 主函数 `generate_ai_move`
```python
def generate_ai_move(game: Game, ai_mode: str, ai_settings: Dict) -> Tuple[Move, GameNode]:
```
- 从注册表获取策略类，实例化。
- 调用 `generate_move()` 得到着法和思考文本。
- 执行 `game.play(move)` 落子，创建新节点，将思考文本存入 `ai_thoughts`。
- 返回着法和节点。

## 9. 设计要点总结
- **策略模式**：通过抽象基类和注册表实现多种 AI 行为，易于扩展。
- **组合与继承**：`OwnershipBaseStrategy` 和 `PickBasedStrategy` 提供通用功能，子类复用。
- **与 KataGo 交互**：可请求额外分析或等待常规分析，决策依赖分析数据。
- **随机性与可调参数**：多数策略引入加权随机选择，参数可调，模拟不同棋力/风格。
- **日志**：大量 `log` 调用，便于调试和理解决策过程。
- **错误处理**：分析失败时回退到默认或策略首着，保证程序健壮。

`ai.py` 是 KaTrain 实现 AI 对手多样性和可配置性的核心模块，充分利用 KataGo 的分析输出，通过权重调整、筛选和随机机制，生成符合特定要求的着法。

# `katrain/core/contribute_engin.py`

该代码定义了一个 `KataGoContributeEngine` 类，用于与 KataGo 的 `contribute` 程序交互，从而参与 KataGo 的分布式训练（贡献自己的算力）。该类继承自 `BaseEngine`，是 KaTrain 中负责引擎通信的模块之一。

## 主要功能
- 启动和管理 KataGo 的 `contribute` 进程。
- 接收 KataGo 输出的 JSON 数据，解析为对局信息，并在 KaTrain 中动态展示。
- 维护多个同时进行的训练对局，支持切换展示不同的对局。
- 统计贡献数据（如已上传对局数、移动数、访问量等）。
- 支持暂停/恢复、优雅退出等操作。

## 关键属性
| 属性 | 说明 |
|------|------|
| `active_games` | 正在进行的对局字典，键为 `gameId`，值为 `BaseGame` 实例。 |
| `finished_games` | 已结束但尚未从 `active_games` 中移除的对局 ID 集合。 |
| `showing_game` | 当前正在 KaTrain 界面上展示的对局 ID。 |
| `uploaded_games_count` | 已成功上传的 SGF 文件数量（即已完成的贡献对局）。 |
| `move_count` / `visits_count` | 累计移动次数和访问量，用于统计速度。 |
| `server_error` | 捕获的服务器错误信息，用于提前终止进程。 |
| `paused` | 是否处于暂停状态。 |
| `save_sgf` / `save_path` | 是否保存 SGF 文件及保存路径。 |
| `move_speed` | 展示对局时的自动播放速度（秒/步）。 |

## 主要方法

### 初始化 `__init__`
- 读取配置文件（`katrain.config("contribute")`），设置相关参数。
- 检查并复制 `cacert.pem` 证书文件（若不存在）。
- 构造 KataGo 的命令行参数（用户名、密码、最大并行游戏数等）。
- 调用 `start()` 启动进程。

### 启动与停止
- `start()`：启动 KataGo 子进程，并创建两个守护线程分别读取 stdout 和 stderr。
- `shutdown(finish)`：向进程发送 `forcequit` 并终止进程。
- `graceful_shutdown()`：发送 `quit` 命令，让 KataGo 完成当前对局后退出。
- `pause()`：切换暂停/继续状态，向进程发送 `pause` 或 `resume`。

### 线程处理
- `_read_stdout_thread()`：循环读取 KataGo 的 stdout，解析 JSON 行。
  - 若为新的 `gameId`，根据 JSON 信息创建 `BaseGame` 实例（包含棋盘大小、规则、玩家名等）。
  - 若为已有对局，调用 `sync_branch` 更新棋局分支，并将分析数据附加到节点上。
  - 更新统计信息（`move_count`、`visits_count`）。
  - 触发 KaTrain 界面刷新（`self.katrain("update-state")`）。
- `_read_stderr_thread()`：读取 stderr，记录日志；若检测到服务器错误（如非 200 状态码），则记录错误并终止。

### 对局展示与推进
- `advance_showing_game()`：控制当前展示的对局自动播放。
  - 若当前展示的对局已结束，等待 `SHOW_RESULT_TIME` 后将其从 `active_games` 移除，并保存 SGF（若启用）。
  - 若对局未结束但停留时间超过 `move_speed`，则前进一手（`redo(1)`）。
  - 若对局卡住超过 `GIVE_UP_AFTER`，则放弃该对局，切换到下一个可展示的对局。
  - 选择下一个展示对局的策略：遍历所有活跃对局，选择剩余待展示步数最多的对局。
- `game_ended(game)`：判断对局是否已结束（通过检查当前节点是否为 pass 以及候选着法为空，最终调用 `end_result`）。

### 统计与状态
- `status()`：返回当前贡献状态的摘要字符串（已上传对局数、缓冲区大小、移动速度等）。
- `check_alive()`：检查进程是否存活，若死亡则记录错误。

## 数据流
1. KataGo 通过 stdout 输出 JSON 格式的棋局更新信息。
2. `_read_stdout_thread` 解析 JSON，更新或创建对应的 `BaseGame`。
3. 每次更新后调用 `self.katrain("update-state")` 通知界面刷新。
4. 界面上的自动播放由 `advance_showing_game` 驱动（通常由 KaTrain 的主循环定期调用）。

## 异常处理
- 若 KataGo 进程意外退出，会记录错误（如 DLL 缺失）并清理资源。
- 解析 JSON 出错时会打印堆栈并继续。
- 服务器返回错误时，设置 `server_error` 并终止引擎。

## 注意事项
- 该类依赖于 `BaseGame` 和 `Move` 等 KaTrain 核心类，用于在本地模拟对局树。
- 所有与 KataGo 的通信都是异步的，通过线程读取输出。
- 配置项中的 `movespeed` 控制自动播放速度，`savesgf` 决定是否保存对局文件。

该引擎使得 KaTrain 可以作为一个“贡献客户端”连接到 KataGo 的分布式训练网络，在后台自动进行对局并上传结果，同时允许用户实时观看进行中的棋局。

# `katrain/core/tsumego_frame.py`

该文件 `tsumego_frame.py` 实现了 KaTrain 中“死活题框架”的生成逻辑，其核心功能是根据当前棋盘上的棋子分布，自动构建一个适合死活题分析的局部棋盘，并在棋局树中插入一个新的节点，包含所需的附加棋子（边界、外部填充、劫材等）。代码由 lizgoban 项目移植而来，采用 (i, j) 坐标系（i 为行，j 为列），但在返回给 KaTrain 时会转换为 (j, i) 格式。

## 整体流程
1. 从 KaTrain 的当前棋局（`game`）获取棋盘表示（`bw_board`）和相关参数（komi、黑先、劫存在、边距）。
2. 调用 `tsumego_frame` 生成带有标记信息的棋子网格 `stones`，并从中提取：
   - 题目区域内的黑子、白子坐标列表（`blacks`、`whites`）。
   - 分析区域矩形 `analysis_region`（可能为 `None`）。
3. 将坐标列表转换为 KaTrain 的 SGF 坐标格式（通过 `katrain_sgf_from_ijs`），创建一个新的 `GameNode`，父节点为当前节点，属性 `AB` 和 `AW` 分别设置为框架中的黑子和白子。
4. 返回新节点和分析区域（坐标已转换为 `(j, i)`），供 KaTrain 界面使用。

## 主要函数详解

### `tsumego_frame_from_katrain_game(game, komi, black_to_play_p, ko_p, margin)`
- **作用**：对外接口，从 KaTrain 游戏对象生成死活题框架节点。
- **输入**：
  - `game`：KaTrain 的 `Game` 实例，包含当前棋盘状态。
  - `komi`：贴目。
  - `black_to_play_p`：当前是否轮到黑棋下。
  - `ko_p`：是否存在劫争。
  - `margin`：扩展边距，决定在题目区域外添加多少行/列的辅助棋子。
- **步骤**：
  1. 获取当前节点和棋盘 `bw_board`（二维列表，元素为 `'B'`、`'W'` 或 `'-'`）。
  2. 调用 `tsumego_frame` 获得题目区域的黑白子坐标列表 `blacks`、`whites` 和分析区域。
  3. 将坐标列表转换为 SGF 格式：`katrain_sgf_from_ijs` 使用 `Move.from_gtp` 的逆向过程，生成类似 `"dd"` 的坐标字符串。
  4. 创建新节点 `played_node`，父节点为 `current_node`，属性 `AB` 和 `AW` 分别填入黑白子列表。
  5. 若 `analysis_region` 不为空，将其坐标从 `(i, j)` 转换为 `(j, i)` 后返回。

### `tsumego_frame(bw_board, komi, black_to_play_p, ko_p, margin)`
- **作用**：生成带标记的棋子网格，并提取分析区域。
- **输入**：同前，但 `bw_board` 是已转换为字典格式的 stones（通过 `stones_from_bw_board`）。
- **步骤**：
  1. 调用 `tsumego_frame_stones` 获得填充后的 stones（每个格子是一个字典，可能包含 `stone`、`black`、`tsumego_frame`、`tsumego_frame_region_mark` 等键）。
  2. 使用 `pick_all` 提取具有 `tsumego_frame_region_mark` 的格子（即边界棋子）作为 `region_pos`。
  3. 使用 `pick_all` 提取具有 `tsumego_frame` 的格子（即题目区域内的棋子）并区分黑白，得到 `blacks` 和 `whites`。
  4. 调用 `get_analysis_region` 根据 `region_pos` 计算最小外接矩形，若矩形合法则返回 `(ri, rj)`，否则返回 `None`。

### `tsumego_frame_stones(stones, komi, black_to_play_p, ko_p, margin)`
- **作用**：核心处理函数，对 stones 网格进行标准化、翻转、添加辅助棋子。
- **流程**：
  1. **提取棋子坐标**：遍历 stones，收集所有有棋子的格子（`stone` 为 `True`）及其颜色。
  2. **计算极值**：找出最上、最下、最左、最右的棋子坐标，并调用 `min_by`（带方向参数）获取对应格子。
  3. **吸附边缘**：将极值坐标向棋盘边缘“吸附”，如果距离边缘不超过 `near_to_edge`（默认 2），则设为边缘坐标（0 或 size-1），否则保持不变。目的是使后续翻转操作对称。
  4. **判断是否需要翻转/旋转**：
     - 若 `imin < jmin`，则设置翻转规格 `[False, False, True]`（交换 i、j 轴，不单独翻转）。
     - 否则，分别判断 i 轴和 j 轴是否需要翻转（通过 `need_flip_p`），并设置 `swap=False`。
     - 如果规格中有任一 `True`，则先对 stones 进行翻转（`flip_stones`），递归调用自身处理，再将结果翻转回来。这样保证处理时题目区域位于标准位置（左上角优先）。
  5. **计算扩展范围**：`i0 = imin - margin`，`i1 = imax + margin`，`j0 = jmin - margin`，`j1 = jmax + margin`。
  6. **猜测攻击方**：调用 `guess_black_to_attack` 根据极值点的颜色和位置判断攻击方是黑还是白。
  7. **放置边界棋子**：`put_border` 在矩形的四条边上放置棋子，颜色为攻击方颜色，并标记 `tsumego_frame_region_mark=True`，用于确定分析区域。
  8. **放置外部填充**：`put_outside` 在矩形外部区域（即不在 `[i0,i1]x[j0,j1]` 内）放置模拟全局形势的棋子，颜色分布根据攻击方、komi 和棋盘大小计算。
  9. **放置劫材**：`put_ko_threat` 在矩形的某个角落放置一个劫材图案，图案取决于攻击方、当前轮到谁下以及劫是否存在。
  10. 返回填充后的 stones。

### 辅助函数说明

- **`put_border(stones, sizes, frame_range, is_black)`**  
  在矩形的上下边界和左右边界放置棋子，调用 `put_twin` 分别处理水平边界和垂直边界。

- **`put_outside(stones, sizes, frame_range, black_to_attack_p, black_to_play_p, komi)`**  
  遍历棋盘所有格子，若在矩形外，则按顺序计数，根据 `defense_area` 决定该位置属于攻击方还是防御方。同时加入 `empty_p` 条件使部分格子留空，避免填充过于密集。

- **`put_ko_threat(stones, sizes, frame_range, black_to_attack_p, black_to_play_p, ko_p)`**  
  根据攻击方、劫存在、当前轮次选择劫材图案（`offense_ko_threat` 或 `defense_ko_threat`），解析图案并在棋盘左上或右下角放置棋子，确保不覆盖矩形内部。

- **`flip_stones(stones, flip_spec)`**  
  根据规格 `[flip_i, flip_j, swap]` 对 stones 进行翻转和/或坐标交换，返回新网格。

- **`put_stone(stones, sizes, i, j, black, empty, tsumego_frame_region_mark=False)`**  
  在指定位置放置一个字典，若 `empty` 为 `True` 则放置空字典，否则设置 `stone`、`black`、`tsumego_frame`（总是 `True`）以及可选的 `tsumego_frame_region_mark`。

- **`inside_p(i, j, region)`**  
  判断点是否在矩形 `[i0,i1]x[j0,j1]` 内。

- **`stones_from_bw_board(bw_board)`**  
  将字符串棋盘（如 `[['B','-','W'], ...]`）转换为 stones 字典列表，每个有棋子的格子包含 `{'stone': True, 'black': True/False}`。

- **`pick_all(stones, key)`**  
  提取 stones 中所有含有指定键的格子，返回 `[i, j, black]` 列表。

- **`get_analysis_region(region_pos)`**  
  根据 `region_pos` 中的坐标计算最小行范围和列范围，若范围合法则返回 `((i_min, i_max), (j_min, j_max))`，否则返回 `None`。

- **`guess_black_to_attack(extrema, sizes)`**  
  根据四个极值点的颜色及其到棋盘中心的距离（高度）加权求和，若和 >0 则攻击方为黑，否则为白。这基于一个启发式规则：边界上距离中心远的棋子往往属于进攻方。

- **`height(k, size)`**  
  计算某行或列距离棋盘中心的距离，用于加权。

- **`xor(a, b)`**  
  布尔异或。

## 坐标系说明
- 函数内部统一使用 `(i, j)` 坐标，i 为行索引（从上到下），j 为列索引（从左到右）。
- KaTrain 使用 `(j, i)` 坐标，因此在返回给外部时通过 `katrain_region = (analysis_region[1], analysis_region[0])` 进行转换。

## 整体逻辑总结
该模块通过以下步骤将当前棋盘转化为一个适合死活题分析的局部棋盘：
1. 提取原棋盘中的棋子。
2. 根据棋子分布自动确定题目区域范围。
3. 通过翻转/旋转使题目区域处于标准方向，便于后续对称处理。
4. 在题目区域外围添加一层“边界棋子”，标记出分析区域。
5. 在更外围添加模拟全局形势的填充棋子，以提供正确的全局背景。
6. 在角落添加一个劫材图案，确保死活题分析能正确处理劫争。
7. 将所有这些棋子组合成一个新的棋局节点，插入到棋局树中。

这样，KaTrain 就可以基于这个新节点进行死活题分析，而 KataGo 等引擎能够在一个合理的局部场景下计算最优解。

详解部分
=======
- gaoai.py: _is_valid_move() 
- constants.py
- utils.py: var_to_grid(), weighted_selection_without_replacement()
- ai.py: game_report(), policy_weighted_move(), 
generate_influence_territory_weights(), generate_local_tenuki_weights(), generate_ai_move(), 

# _is_valide_move()

或许是在判断has_liberties时，我们只检查了四个方向，但落子后，如果落子点本身周围没有空点，但落子点所在的新组可能有空点（通过与其他同色棋子的连接），但has_liberties检查的是落子点自身周围，而不是整个组。例如，落子点与同色棋子相连，那么新组的气可能来自其他同色棋子的邻居空点。但我们的has_liberties只检查了落子点周围的四个方向，如果落子点本身周围没有空点，但同色棋子周围有空点，那么新组是有气的。在围棋规则中，落子后，新组的气是整个连通区域的所有空点。所以如果落子与有气的同色棋子连接，那么落子就是合法的，即使它本身周围没有空点。但是我们的_is_valid_move没有考虑这种情况！这是一个重大缺陷。

在_is_valid_move中，我们只检查了落子点自身的四个方向是否有空点（has_liberties），而没有检查落子点连接的同色棋子是否给新组带来气。这可能导致漏判很多合法落子。比如，一个棋子放在己方一块棋的旁边，如果己方那块棋有气，那么落子后新组仍然有气（继承了那些气）。但我们的算法只检查落子点本身周围是否有空点，如果落子点本身被对方棋子包围，但连接到的己方棋子有气，则has_liberties为False，然后检查captures。如果也没有提吃，那么就会返回False，但实际应该合法。这正是问题的关键！当AI选择可以吃棋时，可能同时连接到有气的己方棋子，从而落子后新组有气，不需要提吃。这种情况也应该合法，但我们的_is_valid_move可能将其排除。

所以我们需要修改_is_valid_move，使其能够正确判断落子后新组的整体气。也就是说，需要实现一个函数来计算落子后（考虑提吃）新形成的连通组是否有气。

但当前的_is_valid_move只检查了落子点自身的直接邻居空点和能否提吃对方。这忽略了通过连接己方棋子获得气的情况。

例如，假设己方有一块棋有气，对方有一块棋无气，但己方棋和对方棋相邻，在己方棋旁边空位落子，这个空位可能连接己方有气棋，同时也可能提吃对方无气棋。但即使不提吃，只要连接到有气棋，就合法。我们的算法如果没有提吃，且落子点自身周围无空点，就会返回False，但实际上合法。

因此，我们需要重写_is_valid_move，使用更准确的方法：模拟落子后，提走所有无气的对方棋子，然后检查新形成的组（包括落子点及所有相连的同色棋子）是否有气。如果有气，则合法；否则非法。

这需要实现一个简单的洪水填充（flood fill）来找出连通区域并计算气。由于棋盘不大，可以这样做。

所以解决方案：在_is_valid_move中，进行完整的模拟和提吃，然后检查新组的气。

注意：我们需要在不修改原始棋盘的情况下进行模拟，可以使用一个临时副本，或者使用类似的方法：设置board[row][col] = stone，然后执行提吃，然后检查该位置所在组的气。最后恢复board。但提吃操作会修改board，所以我们需要先复制board。或者我们可以编写一个函数，基于当前board和落子位置，计算合法性。

由于_is_valid_move是在get_legal_moves中被多次调用，复制棋盘可能会影响性能，但棋盘大小最多19x19，复制代价可接受。或者我们可以优化，但先保证正确性。

我们可以这样实现：

```python
def _is_valid_move(self, row, col, stone):
    # 边界检查
    if not (0 <= row < self.board_size and 0 <= col < self.board_size):
        return False

    board = getattr(self.game_state, "board", [])
    if not board or row >= len(board) or col >= len(board[0]) or board[row][col] != 0:
        return False

    # 劫点检查
    ko_point = getattr(self.game_state, "ko_point", None)
    if ko_point and isinstance(ko_point, dict):
        if row == ko_point.get("row", -1) and col == ko_point.get("col", -1):
            return False

    # 复制棋盘进行模拟
    import copy
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
                            if 0 <= nr < self.board_size and 0 <= nc < self.board_size:
                                if temp_board[nr][nc] == 0:
                                    has_liberty = True
                                    break
                                elif temp_board[nr][nc] == opponent and (nr, nc) not in visited:
                                    stack.append((nr, nc))
                        if has_liberty:
                            break
                    if not has_liberty:
                        # 无气则移除整组
                        for (gr, gc) in group_members:
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
```

注意：这个实现中，我们首先落子，然后循环移除所有无气的对方棋子，最后检查新组是否有气。这符合围棋规则。

但是，这样可能会重复很多次，效率较低。但由于每次只检查一个位置，且棋盘不大，应该可以接受。

另外，我们还需要考虑自杀规则：如果落子后，新组无气，且没有提走任何对方棋子，那么落子非法。上述逻辑中，如果没有提走对方，且新组无气，has_liberty为False，返回False。如果提走了对方，那么新组可能有气（因为被提走的点变成空点），上述逻辑中，在移除对方后，会重新计算新组的气，所以正确。

注意：在移除对方后，可能落子组获得了气。所以最后检查的是最终状态。

此外，ko点需要处理：如果在提吃后形成了劫，那么下一个回合禁止在劫点落子。但在_is_valid_move中，我们不需要考虑劫点（因为劫点是由上一手决定的，我们传入的ko_point已经代表了禁止的位置）。所以我们只需检查传入的ko_point即可。

我们还需要确保在修改后，不会引入其他bug。例如，当棋盘为空时，落子应该合法（has_liberty为True）。我们的算法中，落子后，周围没有对方，也没有同色，那么检查新组（只有自己）的邻居是否有空点，如果周围有空点，则has_liberty为True，合法。如果落子在角落，周围空点少，但只要有一个空点就合法。如果没有空点（被对方包围），且没有提吃，则非法。

此修正通过完整模拟落子后的局面（包括提吃）并检查最终新组是否有气，确保了所有合法吃棋（无论是否连接己方有气棋子）都能被正确识别。将此方法替换原实现即可解决问题。

# contants

在 KaTrain（基于 KataGo 的围棋 AI 分析工具）中，`katrain.core.constants` 模块定义了大量常量，用于控制 AI 的行为、策略、输出级别以及内部逻辑。这些常量通常作为参数传递给 AI 引擎或用于配置分析模式。下面按类别详细解释每个常量的作用。

### 一、AI 策略/风格常量
这些常量用于指定 AI 的思考方式、行棋风格或分析侧重点，常作为 `ai_mode` 或类似参数的值。

| 常量名 | 作用说明 |
|--------|----------|
| **AI_DEFAULT** | 默认的 AI 模式，即标准 KataGo 分析，不附加特殊偏好。 |
| **AI_HANDICAP** | 让子棋模式，用于分析让子局面下 AI 的应对策略。 |
| **AI_INFLUENCE** | 偏好外势/厚势的玩法，AI 会倾向于构筑外势而非直接围空。 |
| **AI_INFLUENCE_ELO_GRID** | 结合 ELO 等级分和网格搜索的影响力模式，可能用于更精细的实力评估。 |
| **AI_JIGO** | 模仿“模仿棋”（Jigo）策略，即对称模仿对手的下法（通常用于测试或特殊训练）。 |
| **AI_ANTIMIRROR** | 反模仿棋策略，用于应对模仿棋的下法。 |
| **AI_LOCAL** | 局部作战模式，AI 会优先处理局部战斗而非全局均衡。 |
| **AI_LOCAL_ELO_GRID** | 局部模式的 ELO 网格版本，结合实力评估进行局部决策。 |
| **AI_PICK** | 选择模式，可能指基于特定规则选择行棋点（如 pick 策略）。 |
| **AI_PICK_ELO_GRID** | 选择模式的 ELO 网格版本。 |
| **AI_POLICY** | 直接使用策略网络（policy network）的输出作为行棋依据，而非通过 MCTS 搜索。 |
| **AI_RANK** | 根据等级分（rank）调整 AI 的行棋强度，模拟特定段位水平的棋手。 |
| **AI_SCORELOSS** | 基于目数损失（score loss）的决策模式，优先避免亏损。 |
| **AI_SCORELOSS_ELO** | 结合 ELO 的目数损失模式，用于评估不同实力下的损失敏感度。 |
| **AI_SETTLE_STONES** | 安定棋子模式，AI 会优先处理未安定的棋子（如孤棋）。 |
| **AI_SIMPLE_OWNERSHIP** | 简化版领地判断模式，可能用于快速估算或低资源环境。 |
| **AI_STRENGTH** | 强度模式，直接控制 AI 的棋力（如调整搜索次数或随机性）。 |
| **AI_TENUKI** | 脱先（tenuki）偏好模式，AI 会更倾向于脱先转战他处，而非局部纠缠。 |
| **AI_TENUKI_ELO_GRID** | 脱先模式的 ELO 网格版本。 |
| **AI_TERRITORY** | 偏好实地模式，AI 会优先抢占实地（与 AI_INFLUENCE 相对）。 |
| **AI_TERRITORY_ELO_GRID** | 实地模式的 ELO 网格版本。 |
| **AI_WEIGHTED** | 加权模式，可能指根据多种因素加权计算行棋胜率。 |
| **AI_WEIGHTED_ELO** | 加权模式的 ELO 版本，将等级分作为权重因子之一。 |
| **AI_HUMAN** | 模拟人类棋手的下法，可能引入人类常见的失误或风格。 |
| **AI_PRO** | 模拟职业棋手的下法，追求高胜率和高质量着法。 |

### 二、ELO 相关常量
这些常量用于校准 AI 的等级分或处理 ELO 网格。

| 常量名 | 作用说明 |
|--------|----------|
| **CALIBRATED_RANK_ELO** | 校准后的等级分基准，用于将 AI 的棋力映射到标准 ELO 体系。 |

### 三、输出/调试级别常量
用于控制日志输出的详细程度。

| 常量名 | 作用说明 |
|--------|----------|
| **OUTPUT_DEBUG** | 输出详细的调试信息，用于开发或排查问题。 |
| **OUTPUT_ERROR** | 仅输出错误信息，正常运行时关闭非必要输出。 |

### 四、优先级与额外处理常量
这些常量用于调整 AI 查询的优先级或添加额外分析步骤。

| 常量名 | 作用说明 |
|--------|----------|
| **PRIORITY_EXTRA_AI_QUERY** | 标记额外的 AI 查询具有较高优先级，可能用于实时交互时抢占资源。 |
| **ADDITIONAL_MOVE_ORDER** | 用于指定额外的行棋顺序（move order），例如在变化图分析中手动插入后续着法。 |

### 总结
这些常量共同构成了 KaTrain 灵活配置 AI 行为的接口，允许用户或开发者根据需求选择不同的分析模式（如实地/外势偏好、模拟不同实力水平、调整搜索策略等），并控制输出和优先级。在开发插件或自定义脚本时，理解这些常量的含义有助于精确控制 AI 的行为。

# var_to_grid()

该函数 `var_to_grid` 用于将一维数组（如 KataGo 返回的所有权或策略数据）转换为二维网格格式，方便按棋盘坐标 `(x, y)` 访问对应的值。

### 函数签名
```python
def var_to_grid(array_var: List[T], size: Tuple[int, int]) -> List[List[T]]:
```

### 参数
- **array_var** (`List[T]`)：一维列表，包含棋盘上每个交叉点的数据。数据按 **从下到上、从左到右** 的顺序排列：首先是最后一行（y = height-1）的所有列（x = 0..width-1），然后依次向上直到第一行（y = 0）。
- **size** (`Tuple[int, int]`)：棋盘尺寸，格式为 `(width, height)`，即列数和行数。

### 返回值
- **List[List[T]]**：二维列表，索引方式为 `grid[y][x]`，其中 `y` 从 0（棋盘最上边）到 `height-1`（棋盘最下边），`x` 从 0（最左边）到 `width-1`（最右边）。这样 `grid[y][x]` 就对应棋盘上坐标 `(x, y)` 的值，符合注释说明。

### 实现逻辑
1. **初始化**：  
   - `ix = 0` 用于跟踪 `array_var` 的当前读取位置。  
   - `grid = [[]] * size[1]` 创建一个长度为 `height` 的列表。注意：`[[]] * n` 会使所有元素引用同一个空列表，但后续代码会为每个 `grid[y]` **直接赋值**一个新的列表切片，从而覆盖原有引用，因此不会引起共享问题。不过这种写法容易引发误解，通常建议用列表推导式 `[[] for _ in range(size[1])]` 替代。

2. **按行填充**：  
   - 循环 `for y in range(size[1] - 1, -1, -1)` 从 `y = height-1` 递减到 `0`，即从棋盘最底行开始处理，逐行向上。  
   - 对于当前行 `y`，从 `array_var` 中取出从 `ix` 开始、长度为 `size[0]` 的切片，赋值给 `grid[y]`。该切片包含当前行的所有列值（从左到右）。  
   - 然后 `ix += size[0]`，将索引移动到下一行的起始位置。

3. **返回结果**：  
   - 循环结束后，`grid` 已被正确填充，返回该二维列表。

### 示例
假设棋盘尺寸为 `size = (3, 2)`（3列2行），`array_var` 按顺序包含6个值：  
`[a, b, c, d, e, f]`  
其中：
- 第一段（索引0-2）对应最后一行（y=1）：`a b c`  
- 第二段（索引3-5）对应第一行（y=0）：`d e f`

执行过程：
- 初始 `grid = [[], []]`（两个空列表的引用，但会被覆盖）
- 第一次循环 `y = 1`：`grid[1] = array_var[0:3] = [a, b, c]`，`ix` 变为3  
- 第二次循环 `y = 0`：`grid[0] = array_var[3:6] = [d, e, f]`，`ix` 变为6  
最终 `grid` 为 `[[d, e, f], [a, b, c]]`，即：
  - `grid[0][0] = d` 对应坐标 (0,0) 顶行最左  
  - `grid[0][1] = e` 对应 (1,0)  
  - `grid[1][0] = a` 对应 (0,1) 底行最左  
符合预期。

### 注意事项
- **输入顺序**：必须保证 `array_var` 的数据顺序为 **从底行到顶行、每行从左到右**，否则转换后的网格坐标会错乱。
- **长度检查**：函数未检查 `array_var` 长度是否等于 `width * height`，若输入数据不足或多余，可能导致切片越界或剩余数据被忽略。
- **初始化方式**：虽然 `[[]] * n` 在当前逻辑下可行，但更安全的做法是使用列表推导式，避免潜在的引用陷阱。

该函数在 KaTrain 中用于将 AI 返回的扁平化数据（如所有权图、策略概率）转换为便于可视化和坐标查询的二维网格格式。

# weighted_selection_without_replacement()

这段代码实现了一种高效的**加权不放回抽样**算法。它从一个元组列表（每个元组的第二个元素是权重）中，按照权重比例随机选择指定数量的元素，且每个元素最多被选一次。下面我们逐行解释其原理和细节。

### 代码分解

```python
def weighted_selection_without_replacement(items: List[Tuple], pick_n: int) -> List[Tuple]:
    elt = [(math.log(random.random()) / (item[1] + 1e-18), item) for item in items]
    return [e[1] for e in heapq.nlargest(pick_n, elt)]
```

- **输入**：`items` 是一个列表，每个元素是一个元组，元组的第二个位置必须是数值型权重（例如 `(id, weight)`）。`pick_n` 是需要抽取的元素个数。
- **第1行（列表推导式）**：  
  对每个元素 `item` 生成一个配对 `(key, item)`。  
  `key` 的计算公式为 `math.log(random.random()) / (item[1] + 1e-18)`。  
  - `random.random()` 生成一个 `(0,1]` 区间（实际可能为0，但概率极低）的均匀随机数。  
  - `math.log(...)` 得到负数（因为参数小于1）。  
  - 分母 `item[1] + 1e-18` 是权重加上一个极小值，目的是避免权重为0时除零错误（加上1e-18后，权重0的项会得到一个绝对值极大的负数，几乎不可能被选中）。  
- **第2行**：  
  使用 `heapq.nlargest(pick_n, elt)` 从 `elt` 中找出 `key` 最大的 `pick_n` 个元素（按 `key` 降序），然后提取出这些元素对应的原始 `item` 并返回。  
  若 `pick_n` 大于列表长度，`nlargest` 会返回所有元素。

### 数学原理：指数分布与加权不放回抽样

这个技巧的核心思想是将每个元素的权重转化为一个**指数分布随机变量**的速率参数，然后利用指数分布“最小时间对应最高速率”的性质来模拟不放回抽样。

1. **生成指数分布随机数**  
   对于权重为 \( w_i \) 的元素，生成一个服从指数分布（速率参数 \( \lambda = w_i \)）的随机变量 \( T_i \)：
   \[
   T_i = -\frac{\ln U_i}{w_i}, \quad U_i \sim \text{Uniform}(0,1]
   \]
   这里 \(-\ln U_i\) 是参数为1的指数分布随机数，除以 \( w_i \) 后得到速率 \( w_i \) 的指数分布。

2. **选择最小的 \( T_i \) 对应加权抽样**  
   指数分布有一个重要性质：在多个独立的指数时钟中，**最先响铃**（即 \( T_i \) 最小）的那个时钟，其速率参数越大，被选中的概率就越高。具体地，对于一组指数随机变量 \( T_i \)，它们的最小值对应的 \( i \) 的概率正比于 \( w_i \)。这正是加权抽样所要求的概率。

   因此，若我们生成所有元素的 \( T_i \)，然后取最小的 \( k \) 个 \( T_i \) 对应的元素，就得到了一个加权不放回抽样的结果（因为一旦取出一个元素，剩下的时钟仍在继续，等价于不放回）。

3. **代码中的等价变换**  
   代码中计算的是 \(\frac{\ln U_i}{w_i}\)，这是 \( -T_i \)。  
   - 由于 \(\ln U_i < 0\)，\(\frac{\ln U_i}{w_i}\) 也是负数。  
   - 取 **最大的** \(\frac{\ln U_i}{w_i}\) 等价于取 **最小的** \( T_i \)（因为 \( -T_i \) 最大对应 \( T_i \) 最小）。  
   所以 `heapq.nlargest(pick_n, elt)` 选出的恰好是 \( T_i \) 最小的前 \( pick_n \) 个元素。

   因此，这段代码通过一次排序（实际上是堆操作）就完成了加权不放回抽样，时间复杂度为 \( O(n \log k) \)，非常高效。

---

### 为什么要加 \( 1e-18 \)？

- 当某个元素的权重为0时，理论上它应该永远不会被选中。  
- 如果不加微小量，分母为0会导致除零错误。  
- 加上一个极小的正数（如 \( 10^{-18} \)）后，该元素的键值为 \(\frac{\ln U}{10^{-18}}\)，这是一个绝对值非常大的负数（例如 \(-10^{18} \times |\ln U|\)），在取最大键值时几乎不可能被选到，从而安全地排除了零权重项。

---

### 注意事项

- 该方法要求所有权重为非负数。负权重没有定义，会导致概率解释失效。
- 如果权重非常小（例如接近0），加上 \( 1e-18 \) 会轻微扭曲抽样概率，但影响可忽略不计。
- 如果 `pick_n` 大于列表长度，函数会返回所有元素（因为 `nlargest` 只会返回存在的元素），符合不放回抽样的自然语义。

---

### 总结

这段代码用巧妙的一行生成了带权随机键，并利用堆选择实现了加权不放回抽样。它避免了传统方法（如轮盘赌重复抽样）的线性开销，且易于理解，是许多算法库中常用的技巧。

# game_report()

这段代码定义了一个函数 `game_report(game, thresholds, depth_filter=None)`，用于生成围棋对局中当前玩家（基于当前节点）的统计报告。它沿着主分支遍历节点，分析每个节点的损失、与AI建议的匹配程度，并汇总为每个玩家的准确性、复杂度等指标。下面逐部分解释其功能。

### 函数参数
- `game`: 围棋游戏对象，包含当前节点 `current_node` 及其他信息（如棋盘大小）。
- `thresholds`: 损失阈值列表，用于将连续损失值分类到不同的区间（例如 `[0, 0.1, 0.5, 1, 2, 5]`）。
- `depth_filter`: 可选，用于限制分析的深度范围。默认为 `(0, 1e9)`，表示所有深度。

### 主要步骤

#### 1. 获取主分支上的所有节点
```python
cn = game.current_node
nodes = cn.nodes_from_root
while cn.children:  # main branch
    cn = cn.children[0]
    nodes.append(cn)
```
从当前节点开始，沿着主分支（即每个节点的第一个子节点）一直向下走，收集从根节点到最深主分支节点的所有节点。

#### 2. 计算实际深度过滤范围
```python
x, y = game.board_size
depth_filter = [
    math.ceil(board_frac * x * y) for board_frac in depth_filter or (0, 1e9)
]
```
`depth_filter` 原本可能是棋盘落子比例的元组（如 `(0.1, 0.8)`），将其转换为实际的步数范围：乘以棋盘交叉点总数（`x*y`）并向上取整。若未提供，则默认为 `(0, 1e9)` 即全部深度。

#### 3. 过滤节点
```python
nodes = [
    n for n in nodes
    if n.move and not n.is_root and depth_filter[0] <= n.depth < depth_filter[1]
]
```
只保留有落子、非根节点且在指定深度范围内的节点。

#### 4. 初始化统计容器
```python
histogram = [{"B": 0, "W": 0} for _ in thresholds]
ai_top_move_count = {"B": 0, "W": 0}
ai_approved_move_count = {"B": 0, "W": 0}
player_ptloss = {"B": [], "W": []}
weights = {"B": [], "W": []}
```
- `histogram`: 每个阈值区间一个桶，记录黑（B）和白（W）的节点数量。
- `ai_top_move_count`: 玩家落子恰好是AI首选着法的次数。
- `ai_approved_move_count`: 玩家落子被AI认为合格（前5或损失小）的次数。
- `player_ptloss`: 记录每个玩家每一步的损失值 `points_lost`。
- `weights`: 记录每个玩家每一步的权重 `(weight, adj_weight)`，用于后续加权计算。

#### 5. 遍历每个节点
```python
for n in nodes:
    points_lost = n.points_lost
    if n.points_lost is None:
        continue
    else:
        points_lost = max(0, points_lost)
    bucket = len(thresholds) - 1 - evaluation_class(points_lost, thresholds)
    player_ptloss[n.player].append(points_lost)
    histogram[bucket][n.player] += 1
```
- 获取节点损失 `points_lost`，若为 `None` 则跳过该节点（可能因为未分析），否则确保非负。
- 通过 `evaluation_class` 函数将损失值映射到阈值区间索引（假设返回0表示最佳区间，值越大损失越大），然后用 `len(thresholds)-1 - 索引` 翻转，使桶索引从小到大对应损失从小到大（即桶0为最小损失，桶末为最大损失）。更新直方图。

##### 计算候选着法权重
```python
cands = n.parent.candidate_moves
filtered_cands = [
    d for d in cands if d["order"] < ADDITIONAL_MOVE_ORDER and "prior" in d
]
weight = min(
    1.0,
    sum([max(d["pointsLost"], 0) * d["prior"] for d in filtered_cands])
    / (sum(d["prior"] for d in filtered_cands) or 1e-6),
)
```
- 从父节点获取候选着法列表 `candidate_moves`，每个着法包含 `order`（AI排序）、`prior`（概率或先验权重）、`pointsLost`（预期损失）等信息。
- 过滤出 `order` 小于某个常量 `ADDITIONAL_MOVE_ORDER`（例如5，表示只考虑前几个候选）且包含 `prior` 的着法。
- 计算这些候选着法的加权平均损失（取正损失），分母为 `prior` 总和，结果上限为1.0。这个 `weight` 反映了当前局面的复杂性：候选着法总体损失越大，`weight` 越高。

##### 调整权重
```python
adj_weight = max(0.05, min(1.0, max(weight, points_lost / 4)))
weights[n.player].append((weight, adj_weight))
```
- `adj_weight` 在 `weight` 和 `points_lost/4` 中取较大值，并限制在 `[0.05, 1]` 区间。这样调整后，既考虑了局面的固有复杂性，也融入了实际损失，确保权重不会太低。

##### AI匹配统计
```python
if n.parent.analysis_complete:
    ai_top_move_count[n.player] += int(cands[0]["move"] == n.move.gtp())
    ai_approved_move_count[n.player] += int(
        n.move.gtp()
        in [
            d["move"]
            for d in filtered_cands
            if d["order"] == 0 or (d["pointsLost"] < 0.5 and d["order"] < 5)
        ]
    )
```
- 若父节点分析完成（`analysis_complete` 为真），则：
  - `ai_top_move_count`：检查AI首选着法（`cands[0]["move"]`）是否等于当前节点的实际落子。
  - `ai_approved_move_count`：检查实际落子是否在过滤后的候选着法中，并且满足条件（order=0，或者损失小于0.5且order<5）。这相当于AI认为合格的前5着法或损失极小的着法。

#### 6. 计算加权平均损失
```python
wt_loss = {
    bw: sum(s * aw for s, (w, aw) in zip(player_ptloss[bw], weights[bw]))
    / (sum(aw for _, aw in weights[bw]) or 1e-6)
    for bw in "BW"
}
```
对于每个颜色，用调整权重 `aw` 对损失 `s` 进行加权平均，得到加权损失 `wt_loss`。分母为权重和，避免除零。

#### 7. 生成汇总统计
```python
sum_stats = {
    bw: (
        {
            "accuracy": 100 * 0.75 ** wt_loss[bw],
            "complexity": sum(w for w, aw in weights[bw]) / len(player_ptloss[bw]),
            "mean_ptloss": sum(player_ptloss[bw]) / len(player_ptloss[bw]),
            "weighted_ptloss": wt_loss[bw],
            "ai_top_move": ai_top_move_count[bw] / len(player_ptloss[bw]),
            "ai_top5_move": ai_approved_move_count[bw] / len(player_ptloss[bw]),
        }
        if len(player_ptloss[bw]) > 0
        else {}
    )
    for bw in "BW"
}
```
- `accuracy`：准确度定义为 `100 * 0.75 ** wt_loss[bw]`，加权损失越大准确度越低（指数衰减）。
- `complexity`：平均原始权重（未调整的 `weight`），反映局面的平均复杂性。
- `mean_ptloss`：普通平均损失。
- `weighted_ptloss`：加权平均损失。
- `ai_top_move`：落子匹配AI首选的比例。
- `ai_top5_move`：落子被AI合格着法列表包含的比例。
若某颜色无节点，则返回空字典。

#### 8. 返回结果
```python
return sum_stats, histogram, player_ptloss
```
返回三个值：每个颜色的汇总统计、直方图、原始损失列表。

### 整体功能
该函数用于评估对局中主分支上每个节点的质量，通过损失、AI一致性等指标，最终给出每个玩家的准确性、复杂度、平均损失等统计信息，可用于后续分析或报告生成。

# policy_weighted_move()

这个函数 `policy_weighted_move` 用于在围棋 AI 中根据策略网络的输出（即每个落子点的概率）生成一个“加权随机”的落子。它的目的是在保证一定随机性的同时，避免选择概率太低的点，从而在探索和利用之间取得平衡。下面逐部分解释：

### 函数参数
- **`policy_moves`**：一个列表，每个元素为 `(policy_value, move)`，表示策略网络为每个合法移动给出的概率（`policy_value`）以及对应的 `move` 对象（通常包含落子坐标）。列表已按概率从高到低排序（常见用法）。
- **`lower_bound`**：一个阈值（百分比，例如 `0.05` 表示 5%）。只有概率大于该阈值的移动才会被考虑，用于过滤掉策略认为可能性极低的“噪声”移动。
- **`weaken_fac`**：一个正数（通常 ≥ 0.01），用于调整概率分布的“陡峭度”。它的作用类似于**温度参数**的倒数。计算加权值时，使用 `pv ** (1 / weaken_fac)`，因此：
  - `weaken_fac > 1`：概率分布变得更平坦（低概率的相对权重提高），增加探索。
  - `weaken_fac < 1`：概率分布变得更陡峭（高概率的权重更高），更倾向于利用。

### 函数逻辑

1. **参数保护**  
   ```python
   lower_bound, weaken_fac = max(0, lower_bound), max(0.01, weaken_fac)
   ```
   确保 `lower_bound` 非负，`weaken_fac` 至少为 0.01（避免除零或过度陡峭）。

2. **构造候选列表 `weighted_coords`**  
   ```python
   weighted_coords = [
       (pv, pv ** (1 / weaken_fac), move)
       for pv, move in policy_moves
       if pv > lower_bound and not move.is_pass
   ]
   ```
   遍历所有策略移动，仅保留：
   - 策略值 `pv` 大于 `lower_bound`
   - 且不是 Pass 移动（通常 Pass 需要单独处理，这里显式排除）
   
   每个元素保存为 `(原始概率, 加权概率, 移动对象)`。加权概率用于后续的随机选择。

3. **有候选时进行加权随机选择**  
   ```python
   if weighted_coords:
       top = weighted_selection_without_replacement(weighted_coords, 1)[0]
       move = top[2]
       ai_thoughts = f"Playing policy-weighted random move {move.gtp()} ({top[0]:.1%}) from {len(weighted_coords)} moves above lower_bound of {lower_bound:.1%}."
   ```
   - 调用外部函数 `weighted_selection_without_replacement`，根据 `加权概率` 进行无放回随机采样，这里请求 1 个结果。返回的是一个列表，取第一个元素 `top`。
   - `top[2]` 即为选中的移动对象。
   - 生成解释字符串：包含选中的移动的 GTP 表示、原始策略概率（以百分比显示）、候选数量、以及所用的下限阈值。

4. **无候选时的后备方案**  
   ```python
   else:
       move = policy_moves[0][1]
       ai_thoughts = f"Playing top policy move because no non-pass move > above lower_bound of {lower_bound:.1%}."
   ```
   如果没有符合条件（概率大于阈值且非 Pass）的移动，则直接选择策略列表中的第一个移动（即最高概率的移动），并给出相应解释。

5. **返回结果**  
   ```python
   return move, ai_thoughts
   ```
   返回选中的移动对象和一个描述选择过程的字符串，供外部记录或显示。

### 典型应用场景
- 在基于策略网络的围棋程序中，用于在自对弈或实战中选择落子。
- 通过调整 `weaken_fac` 可以控制随机性的强度：训练初期可设置较大值鼓励探索，后期可设置较小值强化利用。
- 使用 `lower_bound` 可以避免因采样到策略网络给出的极低概率点而浪费计算资源，同时也有助于减少噪音。

### 依赖的函数
- `weighted_selection_without_replacement(weighted_coords, k)`：假设实现为根据每个元素的加权值（第二项）进行随机抽样，返回 `k` 个无放回的结果（可能采用轮盘赌或更高效的算法）。该函数未在本片段中给出，但常见于随机抽样库或自定义实现中。

这个函数体现了围棋 AI 中常用的“概率加权随机选择”策略，兼具策略的指导性和一定的探索性。

# generate_influence_territory_weights()

这段代码定义了一个函数 `generate_influence_territory_weights`，它的主要作用是根据**影响模式**（`AI_INFLUENCE`）或**领地模式**（其他情况），结合策略网络输出的概率分布（`policy_grid`），为棋盘上的每个合法落子点计算一个**加权值**，并返回一个包含这些加权信息的列表以及一条解释性文本。这种加权常用于围棋 AI 的决策过程中，以引导落子偏好（例如偏向中央或偏向边角）。

### 函数参数

- **`ai_mode`**  
  表示当前模式，通常是一个常量（如 `AI_INFLUENCE` 表示影响模式，其他值表示领地模式）。模式决定了权重计算公式的不同。

- **`ai_settings`**  
  一个字典，包含两个关键配置：
  - `"threshold"`：阈值，表示从棋盘边界数起的**第几线**（1-based）。例如 `threshold=3` 表示第 3 线以内的区域（即距离边界小于 3 的位置）。
  - `"line_weight"`：线权重因子，用于控制加权的衰减强度。其值通常大于 1，此时 `1/line_weight` 作为底数，指数越大，权重越小，实现衰减；若值小于 1，则底数大于 1，反而会放大权重（但常规使用中常设为大于 1 以实现衰减）。

- **`policy_grid`**  
  一个二维数组，形状为 `[size[1]][size[0]]`，通常由策略网络输出，表示每个交叉点被选中的概率（或原始分值）。其中 `policy_grid[y][x]` 对应坐标 `(x, y)` 的值（注意索引顺序：先行后列）。

- **`size`**  
  棋盘尺寸，如 `(19, 19)` 或 `(9, 9)`。

### 函数逻辑详解

1. **计算阈值线（0‑based）**  
   ```python
   thr_line = ai_settings["threshold"] - 1
   ```
   将用户指定的阈值（从 1 开始计数）转换为 0‑based 索引，便于后续与到边界的距离比较。

2. **根据模式定义权重函数 `weight(x, y)`**  
   - **影响模式（`AI_INFLUENCE`）**  
     权重公式为：  
     \[
     \text{weight}(x,y) = \left(\frac{1}{\text{line\_weight}}\right)^{\max(0, \text{thr\_line} - d_x) + \max(0, \text{thr\_line} - d_y)}
     \]
     其中：
     - \(d_x = \min(x, \text{size}[0]-1-x)\)，即点 `(x,y)` 到棋盘左右边界的最短距离。
     - \(d_y = \min(y, \text{size}[1]-1-y)\)，即点到棋盘上下边界的最短距离。
     - 指数是 \(d_x\) 和 \(d_y\) 分别与阈值 `thr_line` 的“不足部分”之和。只有当距离小于阈值时，才会产生正值，否则为 0。  
     该模式下，**距离边界越近的点（即 \(d_x\) 或 \(d_y\) 小于 `thr_line`）权重被衰减**，越靠近中央的点权重保持为 1（因为指数为 0）。这体现了“影响”模式希望 AI 更注重中央区域，避免过早陷入边角。

   - **领地模式（其他情况）**  
     权重公式为：  
     \[
     \text{weight}(x,y) = \left(\frac{1}{\text{line\_weight}}\right)^{\max(0, d_{\min} - \text{thr\_line})}
     \]
     其中：
     - \(d_{\min} = \min(x, \text{size}[0]-1-x, y, \text{size}[1]-1-y)\)，即点到四条边的最短距离。
     - 指数是 \(d_{\min}\) 超过阈值的部分（即 `max(0, d_min - thr_line)`）。只有当点离所有边界都较远（即 \(d_{\min} > \text{thr\_line}\)）时，才会产生正值。  
     该模式下，**距离边界较远的中央区域权重被衰减**，而靠近边界的点（\(d_{\min} \le \text{thr\_line}\)）权重为 1。这符合“领地”模式希望 AI 更注重实地、边角区域的意图。

3. **构建加权坐标列表 `weighted_coords`**  
   ```python
   weighted_coords = [
       (policy_grid[y][x] * weight(x, y), weight(x, y), x, y)
       for x in range(size[0])
       for y in range(size[1])
       if policy_grid[y][x] > 0
   ]
   ```
   遍历棋盘上所有非零策略值的点（`policy_grid[y][x] > 0`），为每个点生成一个四元组：
   - 第一项：**加权策略值** = 原始策略值 × 权重。后续可用于加权随机选择（例如按此值进行抽样）。
   - 第二项：**权重值**，单独保留便于调试或分析。
   - 第三、四项：点的坐标 `(x, y)`。

4. **生成解释文本**  
   ```python
   ai_thoughts = f"Generated weights for {ai_mode} according to weight factor {ai_settings['line_weight']} and distance from {thr_line + 1}th line. "
   ```
   记录当前使用的模式、线权重因子和阈值线信息，便于日志或界面显示。

5. **返回结果**  
   返回 `(weighted_coords, ai_thoughts)`，供上层函数进行后续决策（如基于加权概率抽样落子）。

### 典型应用场景

- **围棋 AI 的落子选择**：在基于策略网络的 AI 中，策略网络输出每个落子点的概率分布。有时需要根据棋局阶段或特定策略调整这种分布，例如在布局阶段更注重中央势力（影响模式），或在中后盘更注重实地（领地模式）。此函数通过位置加权实现这种调整。
- **自对弈数据生成**：在强化学习训练中，可能引入这种加权来增加落子的多样性，引导 AI 探索不同风格。
- **分析工具**：单独使用加权后的列表可以分析 AI 对棋盘各区域的重视程度。

### 关键参数理解

- **`threshold`**：决定加权生效的边界距离。值越小，受影响的区域越靠近边界（影响模式）或越靠近中央（领地模式）。
- **`line_weight`**：控制衰减的“陡峭度”。值越大（底数越小），衰减越快，区域内的权重差异越明显；值接近 1 时，衰减几乎消失，加权近似于无。

总之，这个函数是围棋 AI 中一种灵活的**位置偏好调节工具**，通过简单的指数加权将策略输出与空间位置结合，以适应不同战术需求。

# generate_local_tenuki_weights() 

这个函数 `generate_local_tenuki_weights` 用于根据**局部**（local）或**腾挪**（tenuki）模式，以最近一手棋的位置为中心，生成一个基于高斯分布的权重，从而调整策略网络的输出概率。其核心思想是：若希望 AI 在上一手附近继续落子（局部战斗），则权重随距离衰减；若希望 AI 脱先（腾挪），则权重随距离增加，鼓励远离当前位置的着法。

### 函数参数

- **`ai_mode`**  
  模式标识符，通常为常量（如 `AI_LOCAL` 或 `AI_TENUKI`）。该参数决定权重是**原始高斯权重**还是其**补数**（`1 - 权重`）。

- **`ai_settings`**  
  配置字典，必须包含键 `"stddev"`，表示高斯分布的标准差（单位：棋盘交叉点）。标准差越大，权重分布越平缓，影响范围越广；标准差越小，分布越集中在中心点附近。

- **`policy_grid`**  
  策略网络输出的二维数组，形状为 `[size[1]][size[0]]`，存储每个交叉点的落子概率（或原始得分）。通常 `policy_grid[y][x]` 对应坐标 `(x, y)`。

- **`cn`**  
  上下文对象，必须包含属性 `cn.move`，且 `cn.move.coords` 为当前关注的坐标 `(mx, my)`（通常是上一手棋的位置）。该坐标将作为高斯分布的中心。

- **`size`**  
  棋盘尺寸，例如 `(19, 19)` 或 `(9, 9)`，表示棋盘的宽度和高度。

### 函数逻辑详解

1. **计算方差**  
   ```python
   var = ai_settings["stddev"] ** 2
   ```
   将标准差平方得到方差，用于高斯函数的分母。

2. **获取中心点坐标**  
   ```python
   mx, my = cn.move.coords
   ```
   从上下文对象中提取中心点的 `(x, y)` 坐标。

3. **构建加权坐标列表 `weighted_coords`（基础版本）**  
   ```python
   weighted_coords = [
       (policy_grid[y][x], math.exp(-0.5 * ((x - mx) ** 2 + (y - my) ** 2) / var), x, y)
       for x in range(size[0])
       for y in range(size[1])
       if policy_grid[y][x] > 0
   ]
   ```
   遍历棋盘上所有策略值大于 0 的位置（即至少有一定落子可能性的点），为每个点生成一个四元组：
   - 第一项：原始策略值 `p`。
   - 第二项：高斯权重 `w = exp( -0.5 * distance² / var )`，其中距离平方为 `(x-mx)² + (y-my)²`。该权重在中心点处为 1，随着距离增大按高斯曲线衰减。
   - 第三、四项：坐标 `x, y`。

4. **生成基础解释文本**  
   ```python
   ai_thoughts = f"Generated weights based on one minus gaussian with variance {var} around coordinates {mx},{my}. "
   ```
   这里文本提到了“one minus gaussian”，但实际上是针对基础权重（高斯本身）。稍后在 `AI_TENUKI` 分支中会重新赋值，但文本内容相同，可能是一个小瑕疵（更合理的做法是区分描述）。我们后续可以指出这一点。

5. **根据模式调整权重（关键逻辑）**  
   ```python
   if ai_mode == AI_TENUKI:
       weighted_coords = [(p, 1 - w, x, y) for p, w, x, y in weighted_coords]
       ai_thoughts = f"Generated weights based on one minus gaussian with variance {var} around coordinates {mx},{my}. "
   ```
   - 若模式为 `AI_TENUKI`（腾挪），则对每个元组进行变换：将第二项权重改为 `1 - w`，即**远离中心的点获得高权重**，而中心附近权重接近 0。这鼓励 AI 选择与上一手位置相距较远的着点，实现“脱先”。
   - 同时重新赋值 `ai_thoughts`，但文本内容与之前相同（仍然写着 “one minus gaussian”），这可能会引起歧义。实际意图可能是根据模式不同给出不同说明，但代码中暂未区分。

   如果模式不是 `AI_TENUKI`（即默认为局部模式），则保留原始的 `weighted_coords`（即高斯权重本身），权重随距离衰减，鼓励在上一手附近落子。

6. **返回结果**  
   ```python
   return weighted_coords, ai_thoughts
   ```
   返回调整后的加权坐标列表和解释文本，供上层函数（如基于加权概率的落子选择）使用。

### 关键概念解释

- **高斯权重**  
  公式 `exp( -0.5 * d² / σ² )` 是标准的高斯函数（未归一化），其值域为 (0, 1]。它能够平滑地表示空间邻近性：距离中心越近，权重越大；距离越远，权重指数衰减。通过调整标准差 `σ`，可以控制“局部”的范围。

- **腾挪模式 (`AI_TENUKI`)**  
  腾挪（tenuki）是围棋术语，意为“脱先”，即放弃当前局部战斗而转投他处。此模式下，使用 `1 - w` 作为权重，使得离上一手越远的位置权重越高，从而鼓励探索全局而非纠缠局部。

- **应用场景**  
  - **局部战斗**：在战斗中，希望 AI 连续在局部作战，可使用原始高斯权重强化中心附近的选择。
  - **布局阶段或需要拓宽思路时**：可通过腾挪模式避免 AI 陷入局部纠缠，增加全局视野。
  - **自对弈数据增强**：在训练阶段引入这种空间加权，可以丰富 AI 的风格多样性。

### 注意事项与潜在改进

- 代码中 `ai_thoughts` 在两种模式下文本完全相同，可能会给调试或日志分析带来混淆。建议在 `AI_TENUKI` 分支中使用不同的描述，例如 `"Generated weights based on one minus gaussian (tenuki mode)..."`。
- 高斯函数中未考虑棋盘边界对距离的影响，但通常这种简化足够有效，因为中心点坐标在棋盘范围内。
- 权重与策略值的结合方式由调用者决定，通常是将两者相乘得到最终的加权值，用于随机抽样（如轮盘赌选择）。此处返回的元组第一项为原始策略值，第二项为空间权重，便于上层灵活组合。

总之，这个函数通过高斯分布实现了空间偏好的灵活调节，是围棋 AI 中引入“局部性”或“全局性”策略的常用手段。

# generate_ai_move()

这段代码定义了一个名为 `generate_ai_move` 的函数，用于根据指定的 AI 策略生成并执行一步棋。它接受当前游戏状态、AI 模式名称和设置参数，返回生成的落子及其对应的游戏节点。

### 参数说明
- `game`: 当前的游戏对象，包含棋盘状态、历史记录等信息。
- `ai_mode`: 字符串，指定要使用的 AI 策略类型（如 "random"、"policy" 等）。
- `ai_settings`: 字典，包含该策略所需的配置参数。

### 函数逻辑
1. **日志记录**：函数首先输出调试日志，标明被调用的 AI 模式。
2. **创建策略实例**：通过 `STRATEGY_REGISTRY`（一个将模式名映射到策略类的字典）获取对应策略类，并用传入的 `game` 和 `ai_settings` 实例化该策略对象。
3. **生成走法**：调用策略对象的 `generate_move()` 方法，得到两个返回值：
   - `move`: 一个 `Move` 对象，表示 AI 选择的落子。
   - `ai_thoughts`: 附加的思考信息（如选点理由、胜率等）。
4. **执行走法**：通过 `game.play(move)` 在游戏对象上执行该落子，这会更新棋盘状态并返回一个新的 `GameNode` 对象（`played_node`），该节点记录了这一步棋后的游戏树信息。
5. **附加思考信息**：将 `ai_thoughts` 保存到 `played_node.ai_thoughts` 属性中，便于后续分析或回放。
6. **返回结果**：最终返回一个元组 `(move, played_node)`，包含生成的落子和对应的游戏节点。

### 典型用途
该函数常用于围棋 AI 程序中，作为高层接口将不同 AI 策略统一封装，并自动完成走法生成、棋盘更新和节点记录的全过程。通过 `STRATEGY_REGISTRY` 实现策略的可插拔，使得新增 AI 模式只需注册相应的策略类即可。