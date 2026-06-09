from xml.etree.ElementTree import Element, SubElement, ElementTree

OUT = "Diagram/Activity_15_Corrected_UML_DrawIO.drawio"

LANES = {
    1: {"x": 40, "w": 300, "centers": [190]},
    2: {"x": 340, "w": 880, "centers": [460, 650, 840, 1030, 1130]},
    3: {"x": 1220, "w": 360, "centers": [1400]},
}

SWIMLANE_STYLE = "swimlane;startSize=40;fillColor=none;html=1;fontStyle=1;align=center;verticalAlign=top;"
ACTION_STYLE = "rounded=1;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;"
DECISION_STYLE = "rhombus;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;"
START_STYLE = "ellipse;fillColor=#000;strokeColor=none;"
END_STYLE = "ellipse;shape=endState;fillColor=#000;strokeColor=#000;"
EDGE_STYLE = "edgeStyle=orthogonalEdgeStyle;rounded=1;html=1;strokeColor=#b85450;fillColor=#f8cecc;endArrow=block;endFill=1;"


def x_for(lane, col, width):
    return LANES[lane]["centers"][col] - width / 2


def node(id_, kind, value, lane, y, col=0, w=None, h=None):
    if kind == "start":
        w, h = 30, 30
        style = START_STYLE
    elif kind == "end":
        w, h = 30, 30
        style = END_STYLE
    elif kind == "merge":
        w, h = 40, 40
        style = DECISION_STYLE
    elif kind == "decision":
        w = w or 120
        h = h or 70
        style = DECISION_STYLE
    else:
        w = w or 150
        h = h or 50
        style = ACTION_STYLE
    return {"id": id_, "kind": kind, "value": value, "x": x_for(lane, col, w), "y": y, "w": w, "h": h, "style": style}


def edge(src, dst, label=""):
    return {"source": src, "target": dst, "value": label}


def add_cell(root, id_, value, style, x=None, y=None, w=None, h=None, vertex=False, edge_=False, parent="1", source=None, target=None):
    attrs = {"id": id_, "value": value, "style": style, "parent": parent}
    if vertex:
        attrs["vertex"] = "1"
    if edge_:
        attrs["edge"] = "1"
        attrs["source"] = source
        attrs["target"] = target
    cell = SubElement(root, "mxCell", attrs)
    geo_attrs = {"as": "geometry"}
    if edge_:
        geo_attrs["relative"] = "1"
    else:
        geo_attrs.update({"x": str(round(x, 2)), "y": str(round(y, 2)), "width": str(round(w, 2)), "height": str(round(h, 2))})
    SubElement(cell, "mxGeometry", geo_attrs)


def make_diagram(mxfile, id_, name, lanes, nodes, edges):
    max_y = max(n["y"] + n["h"] for n in nodes) + 70
    height = max(900, int(max_y))
    diagram = SubElement(mxfile, "diagram", {"id": id_, "name": name})
    model = SubElement(
        diagram,
        "mxGraphModel",
        {
            "dx": "1800",
            "dy": "1100",
            "grid": "1",
            "gridSize": "10",
            "guides": "1",
            "tooltips": "1",
            "connect": "1",
            "arrows": "1",
            "fold": "1",
            "page": "1",
            "pageScale": "1",
            "pageWidth": "1620",
            "pageHeight": str(height),
            "math": "0",
            "shadow": "0",
        },
    )
    root = SubElement(model, "root")
    SubElement(root, "mxCell", {"id": "0"})
    SubElement(root, "mxCell", {"id": "1", "parent": "0"})
    for i, title in enumerate(lanes, start=1):
        lane = LANES[i]
        add_cell(root, f"L{i}", title, SWIMLANE_STYLE, lane["x"], 0, lane["w"], height - 10, vertex=True)
    for n in nodes:
        add_cell(root, n["id"], n["value"], n["style"], n["x"], n["y"], n["w"], n["h"], vertex=True)
    for idx, e in enumerate(edges, start=1):
        add_cell(root, f"e{idx}", e["value"], EDGE_STYLE, edge_=True, source=e["source"], target=e["target"])


diagrams = [
    {
        "id": "activity-01",
        "name": "1. Vòng lặp AI kẻ địch",
        "lanes": ["Điều khiển\n(EnemyBase)", "Logic trạng thái\n(IAIState)", "Hệ thống phụ\n(Health/Pool)"],
        "nodes": [
            node("s", "start", "", 1, 60),
            node("m1", "merge", "", 1, 130),
            node("d1", "decision", "Có thể hoạt động?", 1, 210, w=130),
            node("mw", "merge", "", 1, 1110),
            node("a1", "action", "Chờ khung hình", 1, 1180),
            node("d2", "decision", "Thấy người chơi?", 2, 310, 0, w=130),
            node("a2", "action", "Tuần tra", 2, 420, 0),
            node("d3", "decision", "Trong tầm đánh?", 2, 530, 1, w=130),
            node("a3", "action", "Đuổi theo", 2, 640, 1),
            node("d4", "decision", "Đã hồi chiêu?", 2, 750, 2),
            node("a4", "action", "Giữ khoảng cách", 2, 860, 2, w=160),
            node("a5", "action", "Tấn công", 2, 860, 3),
            node("m2", "merge", "", 2, 970, 2),
            node("d5", "decision", "Kẻ địch còn sống?", 2, 1060, 2, w=140),
            node("a6", "action", "Phát hoạt ảnh chết", 3, 1170, w=170),
            node("e", "end", "", 3, 1280),
        ],
        "edges": [
            edge("s", "m1"), edge("m1", "d1"), edge("d1", "mw", "Không"), edge("mw", "a1"), edge("a1", "m1"),
            edge("d1", "d2", "Có"), edge("d2", "a2", "Không"), edge("d2", "d3", "Có"), edge("d3", "a3", "Không"),
            edge("d3", "d4", "Có"), edge("d4", "a4", "Không"), edge("d4", "a5", "Có"), edge("a2", "m2"),
            edge("a3", "m2"), edge("a4", "m2"), edge("a5", "m2"), edge("m2", "d5"), edge("d5", "mw", "Có"),
            edge("d5", "a6", "Không"), edge("a6", "e"),
        ],
    },
    {
        "id": "activity-02",
        "name": "2. Chuyển pha Ogre Boss",
        "lanes": ["Điều khiển\n(OgreBoss)", "Logic pha Boss", "UI và tiến trình"],
        "nodes": [
            node("s", "start", "", 1, 60), node("m1", "merge", "", 1, 130), node("d1", "decision", "Boss còn sống?", 1, 210),
            node("a1", "action", "Kích hoạt chiến thắng", 3, 260, w=180), node("e", "end", "", 3, 360),
            node("a2", "action", "Đọc máu hiện tại", 2, 210, 0, w=160), node("d2", "decision", "Máu dưới ngưỡng ba?", 2, 320, 0, w=150),
            node("a3", "action", "Chuyển pha ba", 2, 430, 0), node("d3", "decision", "Máu dưới ngưỡng hai?", 2, 430, 1, w=150),
            node("a4", "action", "Chuyển pha hai", 2, 540, 1), node("a5", "action", "Giữ pha hiện tại", 2, 540, 2, w=160),
            node("m2", "merge", "", 2, 650, 1), node("a6", "action", "Cập nhật chiến thuật", 2, 730, 1, w=170),
            node("d4", "decision", "Người chơi trong tầm?", 2, 830, 1, w=150), node("a7", "action", "Thực hiện đòn đánh", 2, 940, 0, w=170),
            node("a8", "action", "Di chuyển tiếp cận", 2, 940, 2, w=170), node("m3", "merge", "", 2, 1050, 1),
            node("a9", "action", "Chờ khung hình", 1, 1130),
        ],
        "edges": [
            edge("s", "m1"), edge("m1", "d1"), edge("d1", "a1", "Không"), edge("a1", "e"), edge("d1", "a2", "Có"),
            edge("a2", "d2"), edge("d2", "a3", "Có"), edge("d2", "d3", "Không"), edge("d3", "a4", "Có"),
            edge("d3", "a5", "Không"), edge("a3", "m2"), edge("a4", "m2"), edge("a5", "m2"), edge("m2", "a6"),
            edge("a6", "d4"), edge("d4", "a7", "Có"), edge("d4", "a8", "Không"), edge("a7", "m3"), edge("a8", "m3"),
            edge("m3", "a9"), edge("a9", "m1"),
        ],
    },
    {
        "id": "activity-03",
        "name": "3. Lưu và tải lại",
        "lanes": ["Điều khiển\nSaveLoad", "Dịch vụ dữ liệu", "Lưu trữ\nLocal/Cloud"],
        "nodes": [
            node("s", "start", "", 1, 60), node("d1", "decision", "Đã đăng nhập?", 1, 140),
            node("m0", "merge", "", 3, 250), node("a1", "action", "Tải dữ liệu cục bộ", 3, 330, w=170),
            node("a2", "action", "Gọi tải cloud", 2, 240, 0), node("d2", "decision", "Cloud thành công?", 2, 340, 0, w=140),
            node("a3", "action", "Nhận dữ liệu cloud", 3, 440, w=170), node("a4", "action", "Ghi lỗi đồng bộ", 2, 460, 0, w=160),
            node("m1", "merge", "", 3, 560), node("d3", "decision", "Có dữ liệu lưu?", 2, 650, 1, w=140),
            node("a5", "action", "Tạo dữ liệu mặc định", 2, 760, 0, w=180), node("a6", "action", "Giải mã dữ liệu lưu", 2, 760, 2, w=170),
            node("m2", "merge", "", 2, 870, 1), node("a7", "action", "Áp dụng trạng thái", 1, 960, w=170),
            node("a8", "action", "Cập nhật giao diện", 3, 1060, w=170), node("e", "end", "", 3, 1160),
        ],
        "edges": [
            edge("s", "d1"), edge("d1", "m0", "Không"), edge("m0", "a1"), edge("d1", "a2", "Có"), edge("a2", "d2"),
            edge("d2", "a3", "Có"), edge("d2", "a4", "Không"), edge("a4", "m0"), edge("a1", "m1"), edge("a3", "m1"),
            edge("m1", "d3"), edge("d3", "a5", "Không"), edge("d3", "a6", "Có"), edge("a5", "m2"), edge("a6", "m2"),
            edge("m2", "a7"), edge("a7", "a8"), edge("a8", "e"),
        ],
    },
    {
        "id": "activity-04",
        "name": "4. Thêm vật phẩm vào kho",
        "lanes": ["Điều khiển\nInventory", "Logic kho đồ", "UI và lưu dữ liệu"],
        "nodes": [
            node("s", "start", "", 1, 60), node("a1", "action", "Nhận vật phẩm", 1, 140), node("d1", "decision", "Vật phẩm hợp lệ?", 2, 140, 0, w=140),
            node("a2", "action", "Bỏ qua vật phẩm", 3, 140, w=160), node("d2", "decision", "Có thể cộng dồn?", 2, 250, 0, w=140),
            node("a3", "action", "Cộng vào ô cũ", 2, 360, 0), node("d3", "decision", "Còn ô trống?", 2, 360, 1),
            node("a4", "action", "Thêm ô mới", 2, 470, 1), node("a5", "action", "Hiển thị túi đầy", 3, 470, w=160),
            node("m1", "merge", "", 2, 580, 1), node("a6", "action", "Cập nhật kho đồ", 2, 660, 1, w=160),
            node("a7", "action", "Lưu thay đổi", 3, 760), node("a8", "action", "Cập nhật giao diện", 3, 860, w=170), node("e", "end", "", 3, 960),
        ],
        "edges": [
            edge("s", "a1"), edge("a1", "d1"), edge("d1", "a2", "Không"), edge("a2", "e"), edge("d1", "d2", "Có"),
            edge("d2", "a3", "Có"), edge("d2", "d3", "Không"), edge("d3", "a4", "Có"), edge("d3", "a5", "Không"),
            edge("a5", "e"), edge("a3", "m1"), edge("a4", "m1"), edge("m1", "a6"), edge("a6", "a7"), edge("a7", "a8"), edge("a8", "e"),
        ],
    },
    {
        "id": "activity-05",
        "name": "5. Chế tạo vật phẩm",
        "lanes": ["Giao diện\nCrafting", "Logic chế tạo", "Kho đồ"],
        "nodes": [
            node("s", "start", "", 1, 60), node("a1", "action", "Chọn công thức", 1, 140), node("d1", "decision", "Công thức hợp lệ?", 2, 140, 0, w=150),
            node("a2", "action", "Hiển thị lỗi công thức", 3, 140, w=200), node("d2", "decision", "Đủ nguyên liệu?", 2, 250, 0, w=130),
            node("a3", "action", "Hiển thị thiếu nguyên liệu", 3, 250, w=220), node("d3", "decision", "Còn chỗ chứa?", 2, 360, 0, w=130),
            node("a4", "action", "Hiển thị túi đầy", 3, 360, w=160), node("a5", "action", "Trừ nguyên liệu", 2, 470, 0),
            node("a6", "action", "Tạo vật phẩm", 2, 570, 1), node("a7", "action", "Thêm vào kho", 3, 670),
            node("a8", "action", "Cập nhật giao diện", 1, 770, w=170), node("e", "end", "", 3, 870),
        ],
        "edges": [
            edge("s", "a1"), edge("a1", "d1"), edge("d1", "a2", "Không"), edge("a2", "e"), edge("d1", "d2", "Có"),
            edge("d2", "a3", "Không"), edge("a3", "e"), edge("d2", "d3", "Có"), edge("d3", "a4", "Không"), edge("a4", "e"),
            edge("d3", "a5", "Có"), edge("a5", "a6"), edge("a6", "a7"), edge("a7", "a8"), edge("a8", "e"),
        ],
    },
    {
        "id": "activity-06",
        "name": "6. Đặt công trình",
        "lanes": ["Người chơi\nInput", "Logic đặt công trình", "Tài nguyên và lưu"],
        "nodes": [
            node("s", "start", "", 1, 60), node("a1", "action", "Chọn công trình", 1, 140), node("a2", "action", "Tạo bản xem trước", 2, 140, 0, w=170),
            node("m1", "merge", "", 2, 240, 0), node("a3", "action", "Đọc vị trí chuột", 1, 330, w=160),
            node("d1", "decision", "Vị trí hợp lệ?", 2, 330, 0, w=130), node("a4", "action", "Hiển thị màu hợp lệ", 2, 440, 0, w=180),
            node("a5", "action", "Hiển thị màu lỗi", 2, 440, 2, w=160), node("m2", "merge", "", 2, 550, 1),
            node("d2", "decision", "Người chơi xác nhận?", 1, 650, w=150), node("d3", "decision", "Đủ tài nguyên?", 2, 650, 1, w=130),
            node("a6", "action", "Trừ tài nguyên", 3, 650), node("a7", "action", "Đặt công trình", 2, 760, 1),
            node("a8", "action", "Lưu trạng thái", 3, 860), node("a9", "action", "Hiển thị thiếu tài nguyên", 3, 760, w=210),
            node("d4", "decision", "Người chơi hủy?", 1, 840, w=130), node("a10", "action", "Xóa bản xem trước", 1, 960, w=170),
            node("a11", "action", "Chờ thao tác tiếp", 2, 1040, 0, w=160), node("e", "end", "", 3, 980),
        ],
        "edges": [
            edge("s", "a1"), edge("a1", "a2"), edge("a2", "m1"), edge("m1", "a3"), edge("a3", "d1"),
            edge("d1", "a4", "Có"), edge("d1", "a5", "Không"), edge("a4", "m2"), edge("a5", "m2"), edge("m2", "d2"),
            edge("d2", "d3", "Có"), edge("d3", "a6", "Có"), edge("a6", "a7"), edge("a7", "a8"), edge("a8", "e"),
            edge("d3", "a9", "Không"), edge("a9", "e"), edge("d2", "d4", "Không"), edge("d4", "a10", "Có"),
            edge("a10", "e"), edge("d4", "a11", "Không"), edge("a11", "m1"),
        ],
    },
    {
        "id": "activity-07",
        "name": "7. Quy trình nấu ăn",
        "lanes": ["Cooking Tower", "Logic nấu ăn", "Kho đồ và UI"],
        "nodes": [
            node("s", "start", "", 1, 60), node("a1", "action", "Chọn món nấu", 1, 140), node("d1", "decision", "Công thức hợp lệ?", 2, 140, 0, w=150),
            node("a2", "action", "Hiển thị lỗi món", 3, 140, w=160), node("d2", "decision", "Đủ nguyên liệu?", 2, 250, 0, w=130),
            node("a3", "action", "Hiển thị thiếu nguyên liệu", 3, 250, w=220), node("a4", "action", "Trừ nguyên liệu", 2, 360, 0),
            node("a5", "action", "Bắt đầu nấu", 2, 460, 1), node("m1", "merge", "", 2, 560, 1),
            node("d3", "decision", "Đã nấu xong?", 2, 650, 1, w=130), node("a6", "action", "Cập nhật thời gian", 2, 760, 0, w=170),
            node("a7", "action", "Chờ khung hình", 1, 860), node("a8", "action", "Tạo món ăn", 3, 760),
            node("a9", "action", "Thêm vào kho", 3, 860), node("a10", "action", "Cập nhật giao diện", 3, 960, w=170), node("e", "end", "", 3, 1060),
        ],
        "edges": [
            edge("s", "a1"), edge("a1", "d1"), edge("d1", "a2", "Không"), edge("a2", "e"), edge("d1", "d2", "Có"),
            edge("d2", "a3", "Không"), edge("a3", "e"), edge("d2", "a4", "Có"), edge("a4", "a5"), edge("a5", "m1"),
            edge("m1", "d3"), edge("d3", "a6", "Không"), edge("a6", "a7"), edge("a7", "m1"), edge("d3", "a8", "Có"),
            edge("a8", "a9"), edge("a9", "a10"), edge("a10", "e"),
        ],
    },
    {
        "id": "activity-08",
        "name": "8. Chết và hồi sinh",
        "lanes": ["Health", "Logic hồi sinh", "Điều khiển và UI"],
        "nodes": [
            node("s", "start", "", 1, 60), node("m1", "merge", "", 1, 130), node("a1", "action", "Nhận thay đổi máu", 1, 220, w=170),
            node("d1", "decision", "Nhân vật bất tử?", 2, 220, 0, w=140), node("a2", "action", "Bỏ qua sát thương", 3, 220, w=170),
            node("a3", "action", "Cập nhật máu", 2, 330, 0), node("d2", "decision", "Máu còn lại?", 2, 430, 0),
            node("a4", "action", "Cập nhật thanh máu", 3, 430, w=170), node("a5", "action", "Khóa điều khiển", 3, 550),
            node("a6", "action", "Phát hoạt ảnh chết", 2, 650, 1, w=170), node("a7", "action", "Dịch chuyển hồi sinh", 2, 750, 1, w=180),
            node("a8", "action", "Khôi phục máu", 1, 850), node("a9", "action", "Mở điều khiển", 3, 950), node("e", "end", "", 3, 1050),
        ],
        "edges": [
            edge("s", "m1"), edge("m1", "a1"), edge("a1", "d1"), edge("d1", "a2", "Có"), edge("a2", "e"),
            edge("d1", "a3", "Không"), edge("a3", "d2"), edge("d2", "a4", "Có"), edge("a4", "e"), edge("d2", "a5", "Không"),
            edge("a5", "a6"), edge("a6", "a7"), edge("a7", "a8"), edge("a8", "a9"), edge("a9", "e"),
        ],
    },
    {
        "id": "activity-09",
        "name": "9. Đấu trường Boss",
        "lanes": ["Vùng đấu Boss", "Logic trận đấu", "UI và lưu"],
        "nodes": [
            node("s", "start", "", 1, 60), node("a1", "action", "Người chơi vào vùng", 1, 140, w=180), node("a2", "action", "Khóa cổng đấu", 1, 240),
            node("a3", "action", "Sinh boss", 2, 240, 0), node("m1", "merge", "", 2, 340, 0), node("d1", "decision", "Boss còn sống?", 2, 430, 0),
            node("a4", "action", "Cập nhật trận đấu", 2, 540, 0, w=170), node("d2", "decision", "Người chơi chết?", 2, 650, 0, w=140),
            node("a5", "action", "Mở trạng thái thua", 3, 650, w=170), node("a6", "action", "Chờ khung hình", 1, 760),
            node("a7", "action", "Mở cổng thắng", 3, 430), node("a8", "action", "Hiển thị bảng thắng", 3, 530, w=180),
            node("a9", "action", "Lưu tiến trình", 3, 630), node("e", "end", "", 3, 820),
        ],
        "edges": [
            edge("s", "a1"), edge("a1", "a2"), edge("a2", "a3"), edge("a3", "m1"), edge("m1", "d1"),
            edge("d1", "a4", "Có"), edge("a4", "d2"), edge("d2", "a5", "Có"), edge("a5", "e"), edge("d2", "a6", "Không"),
            edge("a6", "m1"), edge("d1", "a7", "Không"), edge("a7", "a8"), edge("a8", "a9"), edge("a9", "e"),
        ],
    },
    {
        "id": "activity-10",
        "name": "10. Mở khóa cổng bản đồ",
        "lanes": ["Người chơi", "Logic cổng", "Scene và lưu"],
        "nodes": [
            node("s", "start", "", 1, 60), node("a1", "action", "Người chơi tương tác", 1, 140, w=180),
            node("d1", "decision", "Đủ điều kiện mở?", 2, 140, 0, w=140), node("a2", "action", "Hiển thị yêu cầu", 3, 140, w=160),
            node("d2", "decision", "Cổng đã mở?", 2, 250, 0), node("a3", "action", "Mở khóa cổng", 2, 360, 1),
            node("a4", "action", "Lưu trạng thái cổng", 3, 460, w=180), node("m1", "merge", "", 2, 560, 1),
            node("a5", "action", "Cho phép đi qua", 1, 650, w=160), node("e", "end", "", 3, 650),
        ],
        "edges": [
            edge("s", "a1"), edge("a1", "d1"), edge("d1", "a2", "Không"), edge("a2", "e"), edge("d1", "d2", "Có"),
            edge("d2", "m1", "Có"), edge("d2", "a3", "Không"), edge("a3", "a4"), edge("a4", "m1"), edge("m1", "a5"), edge("a5", "e"),
        ],
    },
    {
        "id": "activity-11",
        "name": "11. Ngủ tại điểm lưu",
        "lanes": ["Điểm ngủ", "Logic nghỉ ngơi", "Lưu Local/Cloud"],
        "nodes": [
            node("s", "start", "", 1, 60), node("a1", "action", "Tương tác điểm ngủ", 1, 140, w=180), node("d1", "decision", "Có thể ngủ?", 2, 140, 0),
            node("a2", "action", "Bỏ qua tương tác", 3, 140, w=160), node("a3", "action", "Hiển thị xác nhận", 1, 250, w=170),
            node("d2", "decision", "Xác nhận ngủ?", 2, 250, 0, w=130), node("a4", "action", "Đóng xác nhận", 3, 250),
            node("a5", "action", "Lưu cục bộ", 3, 360), node("d3", "decision", "Có đăng nhập cloud?", 2, 470, 0, w=150),
            node("a6", "action", "Đồng bộ cloud", 3, 470), node("d4", "decision", "Đồng bộ thành công?", 3, 580, w=150),
            node("a7", "action", "Ghi lỗi đồng bộ", 3, 700, w=160), node("m1", "merge", "", 2, 810, 1),
            node("a8", "action", "Chuyển sang ngày mới", 2, 900, 1, w=180), node("a9", "action", "Khôi phục nhân vật", 1, 1000, w=170), node("e", "end", "", 3, 1000),
        ],
        "edges": [
            edge("s", "a1"), edge("a1", "d1"), edge("d1", "a2", "Không"), edge("a2", "e"), edge("d1", "a3", "Có"),
            edge("a3", "d2"), edge("d2", "a4", "Không"), edge("a4", "e"), edge("d2", "a5", "Có"), edge("a5", "d3"),
            edge("d3", "m1", "Không"), edge("d3", "a6", "Có"), edge("a6", "d4"), edge("d4", "m1", "Có"), edge("d4", "a7", "Không"),
            edge("a7", "m1"), edge("m1", "a8"), edge("a8", "a9"), edge("a9", "e"),
        ],
    },
    {
        "id": "activity-12",
        "name": "12. Nhặt vật phẩm",
        "lanes": ["Người chơi\nCollider", "Logic nhặt đồ", "Kho và Pool"],
        "nodes": [
            node("s", "start", "", 1, 60), node("a1", "action", "Phát hiện vật phẩm", 1, 140, w=170),
            node("d1", "decision", "Trong tầm nhặt?", 2, 140, 0, w=140), node("a2", "action", "Bỏ qua vật phẩm", 3, 140, w=160),
            node("d2", "decision", "Kho còn chỗ?", 2, 250, 0), node("a3", "action", "Hiển thị túi đầy", 3, 250, w=160),
            node("a4", "action", "Thêm vào kho", 2, 360, 0), node("d3", "decision", "Thêm thành công?", 2, 460, 0, w=140),
            node("a5", "action", "Hiển thị lỗi nhặt", 3, 460, w=160), node("a6", "action", "Phát âm thanh nhặt", 1, 570, w=170),
            node("a7", "action", "Ẩn vật phẩm", 3, 670), node("a8", "action", "Trả về pool", 3, 770), node("e", "end", "", 3, 870),
        ],
        "edges": [
            edge("s", "a1"), edge("a1", "d1"), edge("d1", "a2", "Không"), edge("a2", "e"), edge("d1", "d2", "Có"),
            edge("d2", "a3", "Không"), edge("a3", "e"), edge("d2", "a4", "Có"), edge("a4", "d3"), edge("d3", "a5", "Không"),
            edge("a5", "e"), edge("d3", "a6", "Có"), edge("a6", "a7"), edge("a7", "a8"), edge("a8", "e"),
        ],
    },
    {
        "id": "activity-13",
        "name": "13. Phiên đăng nhập",
        "lanes": ["Client Auth", "Backend Auth", "Save Bootstrap"],
        "nodes": [
            node("s", "start", "", 1, 60), node("a1", "action", "Gửi yêu cầu đăng nhập", 1, 140, w=200),
            node("d1", "decision", "Server phản hồi?", 2, 140, 0, w=140), node("a2", "action", "Chuyển sang offline", 3, 140, w=170),
            node("d2", "decision", "Người dùng tồn tại?", 2, 250, 0, w=150), node("a3", "action", "Nhận mã người dùng", 2, 360, 0, w=170),
            node("a4", "action", "Tạo người dùng mới", 2, 360, 2, w=170), node("a5", "action", "Nhận mã người dùng", 2, 460, 2, w=170),
            node("m1", "merge", "", 2, 570, 1), node("a6", "action", "Lưu phiên đăng nhập", 1, 660, w=180),
            node("a7", "action", "Tải dữ liệu cloud", 3, 660, w=160), node("d3", "decision", "Tải thành công?", 3, 760, w=130),
            node("a8", "action", "Áp dụng dữ liệu cloud", 3, 870, w=190), node("a9", "action", "Dùng dữ liệu cục bộ", 1, 870, w=170), node("e", "end", "", 3, 980),
        ],
        "edges": [
            edge("s", "a1"), edge("a1", "d1"), edge("d1", "a2", "Không"), edge("a2", "e"), edge("d1", "d2", "Có"),
            edge("d2", "a3", "Có"), edge("d2", "a4", "Không"), edge("a4", "a5"), edge("a3", "m1"), edge("a5", "m1"),
            edge("m1", "a6"), edge("a6", "a7"), edge("a7", "d3"), edge("d3", "a8", "Có"), edge("d3", "a9", "Không"),
            edge("a8", "e"), edge("a9", "e"),
        ],
    },
    {
        "id": "activity-14",
        "name": "14. Vòng đời hiệu ứng",
        "lanes": ["Nguồn hiệu ứng", "Logic hiệu ứng", "Chỉ số và UI"],
        "nodes": [
            node("s", "start", "", 1, 60), node("a1", "action", "Nhận hiệu ứng", 1, 140), node("d1", "decision", "Mục tiêu miễn nhiễm?", 2, 140, 0, w=150),
            node("a2", "action", "Bỏ qua hiệu ứng", 3, 140, w=160), node("a3", "action", "Áp dụng hiệu ứng", 2, 250, 0, w=160),
            node("m1", "merge", "", 2, 350, 0), node("d2", "decision", "Hiệu ứng còn hạn?", 2, 440, 0, w=150),
            node("a4", "action", "Gỡ hiệu ứng", 3, 440), node("a5", "action", "Khôi phục chỉ số", 3, 540, w=160),
            node("d3", "decision", "Đến lượt tác động?", 2, 550, 1, w=150), node("a6", "action", "Gây tác động định kỳ", 2, 660, 1, w=190),
            node("m2", "merge", "", 2, 770, 1), node("a7", "action", "Chờ khung hình", 1, 860), node("e", "end", "", 3, 650),
        ],
        "edges": [
            edge("s", "a1"), edge("a1", "d1"), edge("d1", "a2", "Có"), edge("a2", "e"), edge("d1", "a3", "Không"),
            edge("a3", "m1"), edge("m1", "d2"), edge("d2", "a4", "Không"), edge("a4", "a5"), edge("a5", "e"),
            edge("d2", "d3", "Có"), edge("d3", "a6", "Có"), edge("d3", "m2", "Không"), edge("a6", "m2"), edge("m2", "a7"), edge("a7", "m1"),
        ],
    },
    {
        "id": "activity-15",
        "name": "15. Đòn đập búa",
        "lanes": ["Điều khiển\nĐòn đánh", "Logic kỹ năng", "Sát thương"],
        "nodes": [
            node("s", "start", "", 1, 60), node("m1", "merge", "", 1, 130), node("d1", "decision", "Đã hồi chiêu?", 1, 210),
            node("a1", "action", "Chờ hồi chiêu", 1, 330), node("d2", "decision", "Có mục tiêu?", 2, 210, 0),
            node("a2", "action", "Hủy đòn đánh", 3, 210), node("a3", "action", "Phát hoạt ảnh đập", 2, 320, 0, w=170),
            node("d3", "decision", "Mục tiêu trong vùng?", 2, 430, 0, w=150), node("a4", "action", "Gây sát thương vùng", 3, 430, w=180),
            node("a5", "action", "Đòn đánh trượt", 2, 540, 1, w=150), node("m2", "merge", "", 2, 650, 1),
            node("a6", "action", "Đặt lại hồi chiêu", 1, 740, w=160), node("e", "end", "", 3, 740),
        ],
        "edges": [
            edge("s", "m1"), edge("m1", "d1"), edge("d1", "a1", "Không"), edge("a1", "m1"), edge("d1", "d2", "Có"),
            edge("d2", "a2", "Không"), edge("a2", "e"), edge("d2", "a3", "Có"), edge("a3", "d3"),
            edge("d3", "a4", "Có"), edge("d3", "a5", "Không"), edge("a4", "m2"), edge("a5", "m2"), edge("m2", "a6"), edge("a6", "e"),
        ],
    },
]


mxfile = Element(
    "mxfile",
    {
        "host": "app.diagrams.net",
        "modified": "2026-06-10T00:00:00.000Z",
        "agent": "Codex",
        "version": "24.7.17",
        "type": "device",
    },
)

for diagram in diagrams:
    make_diagram(mxfile, diagram["id"], diagram["name"], diagram["lanes"], diagram["nodes"], diagram["edges"])

ElementTree(mxfile).write(OUT, encoding="utf-8", xml_declaration=False)
print(f"Wrote {OUT} with {len(diagrams)} diagrams")
