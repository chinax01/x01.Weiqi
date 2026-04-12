import sys

# 自定义简单的观察者基类，替代 kivy._event.Observable
class Observable:
    def __init__(self):
        self._event_callbacks = {}

    def fbind(self, name, func, *args):
        """注册一个事件回调（模拟 Kivy 的 fbind 行为）"""
        if name not in self._event_callbacks:
            self._event_callbacks[name] = []
        self._event_callbacks[name].append((func, args))

    def funbind(self, name, func, *args):
        """注销一个事件回调"""
        if name in self._event_callbacks:
            self._event_callbacks[name] = [
                (f, a) for (f, a) in self._event_callbacks[name]
                if not (f == func and a == args)
            ]

    def dispatch(self, name, *args):
        """触发指定名称的所有回调"""
        for func, fargs in self._event_callbacks.get(name, []):
            func(*(fargs + args))


class Lang(Observable):
    observers = []       # 存储需要更新的 UI 组件信息
    callbacks = []       # 语言变化时的额外回调
    FONTS = {"jp": "NotoSansJP-Regular.otf", "tr": "NotoSans-Regular.ttf", "ua": "NotoSans-Regular.ttf"}

    def __init__(self, lang):
        super().__init__()
        self.lang = None
        self.switch_lang(lang)

    def _(self, text):
        """翻译主函数"""
        return self.ugettext(text)

    def set_widget_font(self, widget):
        """
        原本用于设置 Kivy 组件的字体。
        现在仅保留接口，具体实现由外部使用者决定。
        """
        # 示例：如果 widget 有 font_name 属性，可以在这里设置
        # widget.font_name = self.font_name
        pass

    def fbind(self, name, func, *args):
        """
        覆盖父类方法，专门处理 '_' 属性的绑定。
        其他属性绑定仍走父类通用机制。
        """
        if name == "_":
            # 原逻辑：args[0] 是 (widget, property, ...)
            widget, property, *_ = args[0]
            self.observers.append((widget, func, args))
            try:
                self.set_widget_font(widget)
            except Exception as e:
                print(f"设置字体失败: {e}")
        else:
            super().fbind(name, func, *args)

    def funbind(self, name, func, *args):
        """对应的解绑操作"""
        if name == "_":
            widget, *_ = args[0]
            key = (widget, func, args)
            if key in self.observers:
                self.observers.remove(key)
        else:
            super().funbind(name, func, *args)

    def switch_lang(self, lang):
        """切换语言并更新所有绑定的组件"""
        if lang == self.lang:
            return

        self.lang = lang
        self.font_name = self.FONTS.get(lang)  # 仍保留字体名称记录

        # 翻译函数（目前为恒等映射，可后续接入 gettext）
        self.ugettext = lambda s: s

        # 更新所有通过 fbind 注册的 UI 组件
        for widget, func, args in self.observers:
            try:
                # 调用原始回调（通常是 Kivy 属性更新函数）
                func(args[0], None, None)
                self.set_widget_font(widget)
            except ReferenceError:
                pass  # 组件可能已被销毁
            except Exception as e:
                print(f"切换语言时出错: {e}")

        # 调用额外的语言变化回调
        for cb in self.callbacks:
            try:
                cb(self)
            except Exception as e:
                print(f"语言变化回调执行失败: {e}", file=sys.stderr)


DEFAULT_LANGUAGE = "en"
i18n = Lang(DEFAULT_LANGUAGE)


def rank_label(rank):
    """示例工具函数，生成带段/级的等级标签"""
    if rank is None:
        return "??k"

    if rank >= 0.5:
        return f"{rank:.0f}{i18n._('strength:dan')}"
    else:
        return f"{1-rank:.0f}{i18n._('strength:kyu')}"