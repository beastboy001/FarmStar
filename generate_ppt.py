# -*- coding: utf-8 -*-
"""生成《田园之星之歌》项目展示 PPT（作品集/技术汇报风格）"""
from pptx import Presentation
from pptx.util import Inches, Pt, Emu
from pptx.dml.color import RGBColor as RgbColor
from pptx.enum.text import PP_ALIGN, MSO_ANCHOR
from pptx.enum.shapes import MSO_SHAPE
from pptx.oxml.ns import nsmap
from pptx.oxml import parse_xml
from copy import deepcopy
import os

ASSETS = r"C:\Users\钟健翔\.cursor\projects\e-FarmStar1-FarmStar1\assets"
OUT = r"e:\桌面\工作\FarmStar1\FarmStar1\田园之星之歌_项目展示.pptx"

# 截图映射（按文件名特征）
IMG = {
    "title": "9ca57ba98128822e883bab30d6af3d0d-aed0ae3b-6688-48f5-a2e1-f82e5b33b008.png",
    "save": "ca67ca0b817dcdda79d3a3112a6da189-20645857-a6be-4771-aeca-c7232bff0c73.png",
    "create": "e2e2095d54dd82c079a4dda330445cd2-0712286e-8cbf-4c29-9317-06a784d67fbc.png",
    "welcome": "14206bb572c0d070150c9715d73e2fff-9d665319-e214-4db9-bd26-197446f0a835.png",
    "farm_day": "434c02008552d7cfd845f8d360e49cb2-8045364d-4c37-45bb-969b-45f73cb9d746.png",
    "farm_tool": "ff88eb5c0b0ab3d134d9055201c4b659-0a36f596-4547-4fde-b847-289c9d18a78a.png",
    "harvest": "7648c8333e07f99fad7c5a2320df87e2-06cdc91a-f73a-4e82-baf5-0c3d9df21c2d.png",
    "sunset": "f9384328a3c704fd00cb3ccfe865ad3a-7323c7f5-45d6-4028-8f9d-bc1547c54c5f.png",
    "night": "500faed4569f33fe106bd9738fa3019d-26bcf681-1cf8-44fc-89aa-f1da14d7879e.png",
    "bag": "ee3a1d9bbee79cef8f2fa1f341270bcf-2fda145a-65b8-4b68-b38c-6b464d4a0cb7.png",
    "quest": "c9611a3d0d8cfe9c08b37e9da1f50239-601a1953-7c51-477e-9cf7-b163314d025f.png",
    "npc": "1ccd97616d8bda87cb4c0cb8417267e1-fa145887-22f9-4eb3-9c97-08e0d7677303.png",
    "story1": "88e9efca0b8dffb23982f53cfa65c63f-7cd7b1ce-07e7-4531-8154-3392bd735126.png",
    "story2": "f3e05cdb3717d70264ab195c790f1266-55fc148c-8252-4671-9d0b-b71dca37ce5b.png",
    "dusk_npc": "7643bb58eb603dd7b33909b3c8fb97e9-cc011567-4072-4a30-8869-fa3c48b0feb1.png",
}

def img(key):
    path = os.path.join(ASSETS, "c__Users_____AppData_Roaming_Cursor_User_workspaceStorage_f7a24fce5c9e804050c2f7147cbb2317_images_" + IMG[key])
    if not os.path.exists(path):
        # fallback: find by suffix
        for f in os.listdir(ASSETS):
            if IMG[key] in f or f.endswith(IMG[key]):
                return os.path.join(ASSETS, f)
        raise FileNotFoundError(path)
    return path

# 配色：田园绿 + 暖木色（非紫、非奶油陶土风）
C_BG = RgbColor(0xF4, 0xF7, 0xF2)       # 浅绿底
C_DARK = RgbColor(0x1F, 0x2E, 0x24)      # 深绿字
C_GREEN = RgbColor(0x3D, 0x7A, 0x4A)     # 主绿
C_ACCENT = RgbColor(0xC4, 0x8A, 0x2E)    # 麦穗金
C_MUTED = RgbColor(0x5A, 0x6B, 0x5E)     # 次要字
C_WHITE = RgbColor(0xFF, 0xFF, 0xFF)
C_CARD = RgbColor(0xFF, 0xFF, 0xFF)
C_BAR = RgbColor(0x2E, 0x5C, 0x3A)

W, H = Inches(13.333), Inches(7.5)  # 16:9

def set_run(run, size=18, bold=False, color=C_DARK, font="微软雅黑"):
    run.font.size = Pt(size)
    run.font.bold = bold
    run.font.color.rgb = color
    run.font.name = font
    rPr = run._r.get_or_add_rPr()
    # 东亚字体
    from lxml import etree
    ea = rPr.find("{http://schemas.openxmlformats.org/drawingml/2006/main}ea")
    if ea is None:
        ea = etree.SubElement(rPr, "{http://schemas.openxmlformats.org/drawingml/2006/main}ea")
    ea.set("typeface", font)

def add_bg(slide, color):
    shape = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, 0, 0, W, H)
    shape.fill.solid()
    shape.fill.fore_color.rgb = color
    shape.line.fill.background()
    # 置于底层
    spTree = slide.shapes._spTree
    sp = shape._element
    spTree.remove(sp)
    spTree.insert(2, sp)
    return shape

def add_bar(slide, left, top, width, height, color):
    s = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, left, top, width, height)
    s.fill.solid()
    s.fill.fore_color.rgb = color
    s.line.fill.background()
    return s

def add_text(slide, left, top, width, height, text, size=18, bold=False, color=C_DARK, align=PP_ALIGN.LEFT, font="微软雅黑"):
    box = slide.shapes.add_textbox(left, top, width, height)
    tf = box.text_frame
    tf.word_wrap = True
    p = tf.paragraphs[0]
    p.alignment = align
    run = p.add_run()
    run.text = text
    set_run(run, size, bold, color, font)
    return box

def add_lines(slide, left, top, width, height, lines, size=16, color=C_DARK, bold=False, line_space=1.15):
    box = slide.shapes.add_textbox(left, top, width, height)
    tf = box.text_frame
    tf.word_wrap = True
    for i, line in enumerate(lines):
        p = tf.paragraphs[0] if i == 0 else tf.add_paragraph()
        p.alignment = PP_ALIGN.LEFT
        run = p.add_run()
        run.text = line
        set_run(run, size, bold, color)
        p.space_after = Pt(6)
    return box

def add_card(slide, left, top, width, height):
    s = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, left, top, width, height)
    s.fill.solid()
    s.fill.fore_color.rgb = C_CARD
    s.line.color.rgb = RgbColor(0xD5, 0xE0, 0xD8)
    s.line.width = Pt(1)
    return s

def fit_image(slide, path, left, top, max_w, max_h):
    """等比放入区域"""
    from PIL import Image as PILImage
    with PILImage.open(path) as im:
        iw, ih = im.size
    ratio = min(max_w / iw, max_h / ih)
    w, h = iw * ratio, ih * ratio
    # 居中于框内
    x = left + (max_w - w) / 2
    y = top + (max_h - h) / 2
    return slide.shapes.add_picture(path, int(x), int(y), int(w), int(h))

def page_footer(slide, page, total=12):
    add_text(slide, Inches(11.5), Inches(7.1), Inches(1.5), Inches(0.3),
             f"{page} / {total}", size=11, color=C_MUTED, align=PP_ALIGN.RIGHT)
    add_text(slide, Inches(0.4), Inches(7.1), Inches(4), Inches(0.3),
             "田园之星之歌  ·  Unity 项目展示", size=10, color=C_MUTED)

def section_title(slide, title, subtitle=None):
    add_bar(slide, 0, 0, W, Inches(0.08), C_GREEN)
    add_text(slide, Inches(0.5), Inches(0.25), Inches(12), Inches(0.55),
             title, size=28, bold=True, color=C_DARK)
    if subtitle:
        add_text(slide, Inches(0.5), Inches(0.75), Inches(12), Inches(0.35),
                 subtitle, size=14, color=C_MUTED)

def main():
    prs = Presentation()
    prs.slide_width = W
    prs.slide_height = H
    blank = prs.slide_layouts[6]
    total = 11

    # ========== 1 封面 ==========
    s = prs.slides.add_slide(blank)
    add_bg(s, RgbColor(0x1F, 0x2E, 0x24))
    # 右侧大图
    try:
        fit_image(s, img("title"), Inches(5.8), Inches(0.4), Inches(7.2), Inches(6.7))
    except Exception:
        pass
    # 左侧遮罩
    add_bar(s, 0, 0, Inches(6.2), H, RgbColor(0x1F, 0x2E, 0x24))
    add_bar(s, Inches(0.5), Inches(2.0), Inches(0.12), Inches(1.8), C_ACCENT)
    add_text(s, Inches(0.85), Inches(1.9), Inches(5), Inches(0.8),
             "田园之星之歌", size=40, bold=True, color=C_WHITE)
    add_text(s, Inches(0.85), Inches(2.7), Inches(5), Inches(0.4),
             "Song of the Pastoral Star", size=16, color=C_ACCENT)
    add_text(s, Inches(0.85), Inches(3.4), Inches(5), Inches(0.8),
             "3D 农场模拟经营 · Unity 独立项目", size=16, color=RgbColor(0xC8, 0xD5, 0xCC))
    add_text(s, Inches(0.85), Inches(5.8), Inches(5), Inches(0.6),
             "项目展示 / 作品集", size=14, color=RgbColor(0x9A, 0xB0, 0xA0))
    add_text(s, Inches(0.85), Inches(6.3), Inches(5), Inches(0.4),
             "Unity · C# · URP · 数据驱动", size=12, color=RgbColor(0x7A, 0x90, 0x80))

    # ========== 2 目录 ==========
    s = prs.slides.add_slide(blank)
    add_bg(s, C_BG)
    section_title(s, "目录", "CONTENTS")
    items = [
        ("01", "项目概述", "定位、玩法与技术栈"),
        ("02", "核心玩法", "农场经营与完整循环"),
        ("03", "系统模块", "任务 / 对话 / 背包 / 存档"),
        ("04", "技术架构", "分层设计与实现要点"),
        ("05", "画面与表现", "UI、昼夜与叙事"),
        ("06", "总结与展望", "收获与后续方向"),
    ]
    for i, (num, title, desc) in enumerate(items):
        col = i % 2
        row = i // 2
        x = Inches(0.6 + col * 6.3)
        y = Inches(1.5 + row * 1.6)
        add_card(s, x, y, Inches(5.9), Inches(1.35))
        add_text(s, x + Inches(0.3), y + Inches(0.25), Inches(1.2), Inches(0.5),
                 num, size=26, bold=True, color=C_GREEN)
        add_text(s, x + Inches(1.5), y + Inches(0.3), Inches(4), Inches(0.4),
                 title, size=20, bold=True, color=C_DARK)
        add_text(s, x + Inches(1.5), y + Inches(0.75), Inches(4), Inches(0.35),
                 desc, size=13, color=C_MUTED)
    page_footer(s, 2, total)

    # ========== 3 项目概述 ==========
    s = prs.slides.add_slide(blank)
    add_bg(s, C_BG)
    section_title(s, "01  项目概述", "一款具备完整经营闭环的 3D 农场模拟游戏")
    # 左文右图
    add_card(s, Inches(0.5), Inches(1.3), Inches(6.2), Inches(5.4))
    add_lines(s, Inches(0.8), Inches(1.55), Inches(5.6), Inches(5), [
        "【项目定位】",
        "Unity 3D 农场模拟经营独立项目，参考星露谷式核心循环，",
        "以低多边形画风呈现田园经营体验。",
        "",
        "【核心体验】",
        "· 开垦土地、种植收获、清除杂物",
        "· NPC 对话与主线任务推进",
        "· 背包商店与金币经济",
        "· 多槽位存档与角色创建",
        "· 游戏时间与昼夜环境变化",
        "",
        "【技术栈】",
        "Unity URP  ·  C#  ·  Cinemachine  ·  ScriptableObject",
        "JSON 本地存档  ·  UGUI  ·  TOD 昼夜系统",
    ], size=14, color=C_DARK)
    try:
        fit_image(s, img("welcome"), Inches(7.0), Inches(1.3), Inches(5.8), Inches(5.4))
    except Exception:
        pass
    page_footer(s, 3, total)

    # ========== 4 核心玩法 ==========
    s = prs.slides.add_slide(blank)
    add_bg(s, C_BG)
    section_title(s, "02  核心玩法", "从清障开垦到种植收获的完整农场循环")
    cards = [
        ("地块经营", "杂物清除 → 翻土 → 播种\n生长计时 → 成熟采摘"),
        ("工具系统", "锄 / 斧 / 镐等工具切换\n对应不同交互目标"),
        ("时间系统", "日 / 季 / 年流逝\n驱动作物与昼夜表现"),
        ("经济循环", "收获售卖 → 金币\n购买种子与物资"),
    ]
    for i, (t, d) in enumerate(cards):
        x = Inches(0.45 + i * 3.2)
        add_card(s, x, Inches(1.25), Inches(3.0), Inches(2.2))
        add_bar(s, x, Inches(1.25), Inches(3.0), Inches(0.08), C_GREEN)
        add_text(s, x + Inches(0.2), Inches(1.45), Inches(2.6), Inches(0.4),
                 t, size=16, bold=True, color=C_GREEN)
        add_lines(s, x + Inches(0.2), Inches(1.95), Inches(2.6), Inches(1.3),
                  d.split("\n"), size=12, color=C_MUTED)
    try:
        fit_image(s, img("farm_day"), Inches(0.5), Inches(3.7), Inches(6.0), Inches(3.0))
        fit_image(s, img("harvest"), Inches(6.8), Inches(3.7), Inches(6.0), Inches(3.0))
    except Exception:
        pass
    page_footer(s, 4, total)

    # ========== 5 任务与NPC ==========
    s = prs.slides.add_slide(blank)
    add_bg(s, C_BG)
    section_title(s, "03  任务与 NPC 对话", "数据驱动任务 + 对话树联动")
    add_card(s, Inches(0.5), Inches(1.25), Inches(5.5), Inches(5.5))
    add_lines(s, Inches(0.75), Inches(1.5), Inches(5.0), Inches(5.1), [
        "【任务系统】",
        "· ScriptableObject 配置目标与奖励",
        "· 支持接取 / 进行中 / 完成 / 领奖状态",
        "· 种植、收获、清障、对话等目标类型",
        "· HUD 任务指南实时展示进度",
        "",
        "【NPC 对话】",
        "· 靠近提示 + 按键交互",
        "· 对话树分支选项",
        "· 按任务状态切换对话入口",
        "· 与主线（拜访村长等）深度绑定",
        "",
        "【设计价值】",
        "内容与逻辑解耦，便于扩展新任务与 NPC。",
    ], size=14)
    try:
        fit_image(s, img("npc"), Inches(6.3), Inches(1.25), Inches(6.5), Inches(2.6))
        fit_image(s, img("quest"), Inches(6.3), Inches(4.0), Inches(6.5), Inches(2.75))
    except Exception:
        pass
    page_footer(s, 5, total)

    # ========== 6 背包商店存档 ==========
    s = prs.slides.add_slide(blank)
    add_bg(s, C_BG)
    section_title(s, "04  背包 · 商店 · 存档", "经济系统与数据持久化")
    try:
        fit_image(s, img("bag"), Inches(0.4), Inches(1.2), Inches(7.2), Inches(5.6))
        fit_image(s, img("save"), Inches(7.8), Inches(1.2), Inches(5.1), Inches(2.6))
        fit_image(s, img("create"), Inches(7.8), Inches(4.0), Inches(5.1), Inches(2.8))
    except Exception:
        pass
    # 底部要点条
    add_bar(s, Inches(0.4), Inches(6.85), Inches(12.5), Inches(0.2), C_BG)
    page_footer(s, 6, total)
    add_text(s, Inches(0.5), Inches(6.55), Inches(12), Inches(0.35),
             "多槽位存档 · 角色创建（昵称 / 农场名 / 性别） · 背包分类与商店交易 · JSON 本地持久化",
             size=12, color=C_MUTED, align=PP_ALIGN.CENTER)

    # ========== 7 技术架构 ==========
    s = prs.slides.add_slide(blank)
    add_bg(s, C_BG)
    section_title(s, "05  技术架构", "分层架构 · 数据驱动 · 模块解耦")
    layers = [
        ("表现层", "UI 面板 / 角色控制 / 场景交互 / 昼夜表现"),
        ("业务层", "农场 · 背包 · 商店 · 任务 · NPC 对话"),
        ("数据层", "ScriptableObject 配置 · GameData · JSON 存档"),
        ("框架层", "Architecture · IOC · Event · 资源加载缓存"),
    ]
    for i, (t, d) in enumerate(layers):
        y = Inches(1.35 + i * 1.15)
        add_card(s, Inches(0.6), y, Inches(12.1), Inches(1.0))
        add_bar(s, Inches(0.6), y, Inches(0.15), Inches(1.0), C_GREEN if i % 2 == 0 else C_ACCENT)
        add_text(s, Inches(1.0), y + Inches(0.15), Inches(2.5), Inches(0.7),
                 t, size=20, bold=True, color=C_GREEN)
        add_text(s, Inches(3.8), y + Inches(0.28), Inches(8.5), Inches(0.5),
                 d, size=16, color=C_DARK)
    page_footer(s, 7, total)

    # ========== 8 实现要点 ==========
    s = prs.slides.add_slide(blank)
    add_bg(s, C_BG)
    section_title(s, "06  实现要点", "关键技术点与设计取舍")
    points = [
        ("状态机驱动地块", "地块以 Obstacle / Empty / Plowed / Planted 状态流转，交互逻辑内聚，便于扩展作物与工具类型。"),
        ("真实时间生长", "作物以剩余时间计时并写入存档，支持离线生长与读档精确恢复。"),
        ("任务进度解耦", "玩法侧统一上报进度，任务管理器自动匹配进行中目标，降低模块耦合。"),
        ("对话与任务联动", "同一 NPC 按任务状态选择对话入口，避免为每个任务硬编码分支脚本。"),
        ("UI 面板管理", "统一 Panel 生命周期与资源缓存加载，保证登录—选档—进游戏流程稳定。"),
        ("时间驱动环境", "游戏内时钟同步昼夜光照与天空表现，强化田园氛围沉浸感。"),
    ]
    for i, (t, d) in enumerate(points):
        col = i % 2
        row = i // 2
        x = Inches(0.5 + col * 6.4)
        y = Inches(1.3 + row * 1.7)
        add_card(s, x, y, Inches(6.1), Inches(1.5))
        add_text(s, x + Inches(0.25), y + Inches(0.2), Inches(5.6), Inches(0.4),
                 t, size=16, bold=True, color=C_GREEN)
        add_lines(s, x + Inches(0.25), y + Inches(0.65), Inches(5.6), Inches(0.7),
                  [d], size=12, color=C_MUTED)
    page_footer(s, 8, total)

    # ========== 9 画面表现 ==========
    s = prs.slides.add_slide(blank)
    add_bg(s, C_BG)
    section_title(s, "07  画面与叙事表现", "低多边形场景 · 昼夜氛围 · 漫画过场")
    try:
        fit_image(s, img("sunset"), Inches(0.4), Inches(1.2), Inches(4.2), Inches(2.8))
        fit_image(s, img("night"), Inches(4.7), Inches(1.2), Inches(4.2), Inches(2.8))
        fit_image(s, img("story2"), Inches(9.0), Inches(1.2), Inches(3.9), Inches(2.8))
        fit_image(s, img("story1"), Inches(0.4), Inches(4.2), Inches(6.2), Inches(2.5))
        fit_image(s, img("dusk_npc"), Inches(6.9), Inches(4.2), Inches(5.9), Inches(2.5))
    except Exception:
        pass
    page_footer(s, 9, total)

    # ========== 10 功能总览 ==========
    s = prs.slides.add_slide(blank)
    add_bg(s, C_BG)
    section_title(s, "08  功能总览", "已落地模块一览")
    modules = [
        ("农场玩法", "地块交互 / 种植生长 / 工具作业 / 收获反馈"),
        ("背包商店", "物品管理 / 买卖交易 / 金币经济 / UI 同步"),
        ("任务系统", "多目标配置 / 进度推进 / 奖励发放"),
        ("NPC 对话", "交互提示 / 对话树 / 任务状态分支"),
        ("存档系统", "四槽位读写 / 角色创建 / 核心数据持久化"),
        ("UI 与环境", "主界面 HUD / 小地图 / 时间昼夜表现"),
    ]
    for i, (t, d) in enumerate(modules):
        col = i % 3
        row = i // 3
        x = Inches(0.5 + col * 4.2)
        y = Inches(1.5 + row * 2.4)
        add_card(s, x, y, Inches(4.0), Inches(2.1))
        add_bar(s, x, y, Inches(4.0), Inches(0.1), C_ACCENT)
        add_text(s, x + Inches(0.25), y + Inches(0.35), Inches(3.5), Inches(0.45),
                 t, size=18, bold=True, color=C_DARK)
        add_lines(s, x + Inches(0.25), y + Inches(0.95), Inches(3.5), Inches(0.9),
                  [d], size=13, color=C_MUTED)
    page_footer(s, 10, total)

    # ========== 11 总结 ==========
    s = prs.slides.add_slide(blank)
    add_bg(s, RgbColor(0x1F, 0x2E, 0x24))
    add_bar(s, Inches(0.5), Inches(1.8), Inches(0.12), Inches(2.2), C_ACCENT)
    add_text(s, Inches(0.9), Inches(1.7), Inches(11), Inches(0.7),
             "总结与展望", size=32, bold=True, color=C_WHITE)
    add_lines(s, Inches(0.9), Inches(2.6), Inches(11), Inches(2.5), [
        "本项目完整打通「登录选档 → 角色创建 → 农场经营 → 任务推进 → 存档恢复」链路，",
        "重点锻炼了 Unity 客户端系统拆分、数据驱动内容扩展与模块解耦能力。",
        "",
        "后续可扩展方向：动物养殖与建筑、更丰富的季节玩法、热更新内容层实践等。",
    ], size=16, color=RgbColor(0xC8, 0xD5, 0xCC))
    add_text(s, Inches(0.9), Inches(5.5), Inches(11), Inches(0.5),
             "谢谢观看", size=28, bold=True, color=C_ACCENT)
    add_text(s, Inches(0.9), Inches(6.2), Inches(11), Inches(0.4),
             "田园之星之歌  ·  Unity 独立项目展示", size=14, color=RgbColor(0x7A, 0x90, 0x80))

    prs.save(OUT)
    print("OK:", OUT)

if __name__ == "__main__":
    main()
