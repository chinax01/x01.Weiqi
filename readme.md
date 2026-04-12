# 围棋程序

[katrain](https://gitee.com/mirrors_sanderland/katrain)

1. 创建虚拟环境 ~/py3venv，安装 `pip install katrain`
2. 回退安装 `pip install kivy==2.3.0` 以防卡顿。
3. 虚拟环境终端运行 `katrain` 在配置中下载模型等, 默认在 ~/.katrain 中.
4. 如果使用 x01.weiqi/katrain 本地目录, 模型和i18n等要复制到自定义如 data 目录下, 修改 constants.py 中的常量 DATA_FOLDER="~/lab/data", 而 lang.py 中 switch_lang() 和 config.json 文件也要作相应更改：
```json
{
    "engine": {
        "katago": "[需更改]/katago-v1.16.0-eigenavx2-linux-x64",
        "altcommand": "",
        "model": "[需更改]kata1-b18c384nbt-s9996604416-d4316597426.bin.gz",
        "humanlike_model": "[需更改]/b18c384nbt-humanv0.bin.gz",
        "config": "[需更改]/analysis_config.cfg",
        // 略
    }    
}
```
修改后即可卸载 `pip uninstall katrain` 来使用本地的 katrain 了.  
5. 如使用 pip 安装的 katrain, 则将本地 katrain 目录更名为 katrain2 绕过即可。  
6. 运行 run.py 后，选 `启用AI` 即可与 KataGo 对弈了.

# 围棋网站

1. 安装 `pip install -r requirements.txt`， 不含桌面 `pyside6` 等。
2. 运行 `python index.py`
