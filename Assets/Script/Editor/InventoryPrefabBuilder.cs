using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

namespace Script.Editor
{
    public static class InventoryPrefabBuilder
    {
        [MenuItem("Tools/Build Inventory Prefabs")]
        public static void Build() { BuildAll(); }

        public static void BuildAll()
        {
            var slotPrefab = BuildSlotPrefab();
            BuildInventoryCanvas(slotPrefab);
            AssetDatabase.Refresh();
            Debug.Log("[InventoryPrefabBuilder] Done!");
        }

        static GameObject UI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static Button Btn(string label, Transform parent, Color bg)
        {
            var go = UI(label + "Btn", parent);
            go.AddComponent<LayoutElement>().minHeight = 34;
            var img = go.AddComponent<Image>(); img.color = bg;
            var btn = go.AddComponent<Button>();
            var c = btn.colors;
            c.highlightedColor = new Color(Mathf.Min(bg.r+0.12f,1), Mathf.Min(bg.g+0.12f,1), Mathf.Min(bg.b+0.12f,1));
            c.pressedColor     = new Color(Mathf.Max(bg.r-0.06f,0), Mathf.Max(bg.g-0.06f,0), Mathf.Max(bg.b-0.06f,0));
            btn.colors = c;
            var t = UI("Label", go.transform); Stretch(t.GetComponent<RectTransform>());
            var tmp = t.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = 13; tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
            return btn;
        }

        static GameObject BuildSlotPrefab()
        {
            var root = new GameObject("InventorySlotUI");
            root.AddComponent<RectTransform>().sizeDelta = new Vector2(72, 72);
            var bg = root.AddComponent<Image>(); bg.color = new Color(0.18f,0.18f,0.26f);
            var btn = root.AddComponent<Button>();
            var bc = btn.colors;
            bc.normalColor = new Color(0.18f,0.18f,0.26f);
            bc.highlightedColor = new Color(0.30f,0.30f,0.42f);
            bc.pressedColor = new Color(0.10f,0.10f,0.16f);
            btn.colors = bc;

            // Highlight
            var hl = UI("Highlight", root.transform); Stretch(hl.GetComponent<RectTransform>());
            var hlImg = hl.AddComponent<Image>(); hlImg.color = new Color(1f,1f,0.5f,0.20f); hlImg.enabled = false;

            // Icon
            var icon = UI("Icon", root.transform);
            var iconRT = icon.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.1f,0.2f); iconRT.anchorMax = new Vector2(0.9f,0.9f);
            iconRT.offsetMin = iconRT.offsetMax = Vector2.zero;
            var iconImg = icon.AddComponent<Image>(); iconImg.preserveAspect = true; iconImg.enabled = false;

            // Amount
            var amt = UI("AmountText", root.transform);
            var amtRT = amt.GetComponent<RectTransform>();
            amtRT.anchorMin = Vector2.zero; amtRT.anchorMax = Vector2.one;
            amtRT.offsetMin = new Vector2(2,2); amtRT.offsetMax = new Vector2(-2,-2);
            var amtTMP = amt.AddComponent<TextMeshProUGUI>();
            amtTMP.fontSize = 11; amtTMP.fontStyle = FontStyles.Bold;
            amtTMP.alignment = TextAlignmentOptions.BottomRight; amtTMP.color = Color.white; amtTMP.enabled = false;

            // Durability bar
            var durBg = UI("DurabilityBg", root.transform);
            var durBgRT = durBg.GetComponent<RectTransform>();
            durBgRT.anchorMin = new Vector2(0.05f,0); durBgRT.anchorMax = new Vector2(0.95f,0);
            durBgRT.pivot = new Vector2(0.5f,0); durBgRT.sizeDelta = new Vector2(0,5); durBgRT.anchoredPosition = new Vector2(0,4);
            durBg.AddComponent<Image>().color = new Color(0.15f,0.15f,0.15f);
            var durFill = UI("DurabilityFill", durBg.transform); Stretch(durFill.GetComponent<RectTransform>());
            var durImg = durFill.AddComponent<Image>();
            durImg.color = new Color(0.2f,0.85f,0.3f); durImg.type = Image.Type.Filled;
            durImg.fillMethod = Image.FillMethod.Horizontal; durImg.fillAmount = 1f;
            durBg.SetActive(false);

            // Tooltip
            var tt = UI("Tooltip", root.transform);
            var ttRT = tt.GetComponent<RectTransform>();
            ttRT.anchorMin = new Vector2(1,0); ttRT.anchorMax = new Vector2(1,0); ttRT.pivot = new Vector2(0,0);
            ttRT.sizeDelta = new Vector2(150,64); ttRT.anchoredPosition = new Vector2(4,0);
            tt.AddComponent<Image>().color = new Color(0.05f,0.05f,0.10f,0.96f);
            var ttVL = tt.AddComponent<VerticalLayoutGroup>();
            ttVL.padding = new RectOffset(6,6,4,4); ttVL.spacing = 2;
            ttVL.childForceExpandWidth = true; ttVL.childForceExpandHeight = false; ttVL.childControlHeight = true;
            tt.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var ttName = UI("TooltipName", tt.transform); ttName.AddComponent<LayoutElement>().minHeight = 20;
            var ttNameTMP = ttName.AddComponent<TextMeshProUGUI>();
            ttNameTMP.fontSize = 12; ttNameTMP.fontStyle = FontStyles.Bold; ttNameTMP.color = new Color(0.95f,0.85f,0.55f);
            var ttDesc = UI("TooltipDesc", tt.transform); ttDesc.AddComponent<LayoutElement>().minHeight = 16;
            var ttDescTMP = ttDesc.AddComponent<TextMeshProUGUI>();
            ttDescTMP.fontSize = 10; ttDescTMP.color = new Color(0.72f,0.72f,0.72f); ttDescTMP.enableWordWrapping = true;
            tt.SetActive(false);

            var slotUI = root.AddComponent<Script.Inventory.InventorySlotUI>();
            var so = new SerializedObject(slotUI);
            so.FindProperty("icon").objectReferenceValue           = iconImg;
            so.FindProperty("amountText").objectReferenceValue     = amtTMP;
            so.FindProperty("durabilityBar").objectReferenceValue  = durImg;
            so.FindProperty("highlightImage").objectReferenceValue = hlImg;
            so.FindProperty("tooltipRoot").objectReferenceValue    = tt;
            so.FindProperty("tooltipName").objectReferenceValue    = ttNameTMP;
            so.FindProperty("tooltipDesc").objectReferenceValue    = ttDescTMP;
            so.ApplyModifiedProperties();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/InventorySlotUI.prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }

        static void BuildInventoryCanvas(GameObject slotPrefab)
        {
            var root = new GameObject("InventoryCanvas");
            var canvas = root.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 10;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            // CanvasRoot
            var canvasRoot = UI("CanvasRoot", root.transform); Stretch(canvasRoot.GetComponent<RectTransform>());
            // Overlay
            var ov = UI("DimOverlay", canvasRoot.transform); Stretch(ov.GetComponent<RectTransform>());
            ov.AddComponent<Image>().color = new Color(0,0,0,0.45f);

            // Panel
            var panel = UI("InventoryPanel", canvasRoot.transform);
            var panRT = panel.GetComponent<RectTransform>();
            panRT.anchorMin = panRT.anchorMax = panRT.pivot = new Vector2(0.5f,0.5f);
            panRT.sizeDelta = new Vector2(720,540); panRT.anchoredPosition = Vector2.zero;
            panel.AddComponent<Image>().color = new Color(0.10f,0.10f,0.16f,0.97f);

            // Title bar
            var tbar = UI("TitleBar", panel.transform);
            var tbarRT = tbar.GetComponent<RectTransform>();
            tbarRT.anchorMin = new Vector2(0,1); tbarRT.anchorMax = new Vector2(1,1);
            tbarRT.pivot = new Vector2(0.5f,1f); tbarRT.sizeDelta = new Vector2(0,50); tbarRT.anchoredPosition = Vector2.zero;
            tbar.AddComponent<Image>().color = new Color(0.07f,0.07f,0.12f);
            var ttl = UI("TitleText", tbar.transform); Stretch(ttl.GetComponent<RectTransform>());
            var ttlTMP = ttl.AddComponent<TextMeshProUGUI>();
            ttlTMP.text = "INVENTORY"; ttlTMP.fontSize = 21; ttlTMP.fontStyle = FontStyles.Bold;
            ttlTMP.alignment = TextAlignmentOptions.Center; ttlTMP.color = new Color(0.95f,0.85f,0.55f);

            // Close button
            var clo = UI("CloseButton", tbar.transform);
            var cloRT = clo.GetComponent<RectTransform>();
            cloRT.anchorMin = new Vector2(1,0); cloRT.anchorMax = new Vector2(1,1);
            cloRT.pivot = new Vector2(1,0.5f); cloRT.sizeDelta = new Vector2(50,0); cloRT.anchoredPosition = Vector2.zero;
            clo.AddComponent<Image>().color = new Color(0.65f,0.12f,0.12f);
            clo.AddComponent<Button>();
            var cloTxt = UI("Label", clo.transform); Stretch(cloTxt.GetComponent<RectTransform>());
            var cloTMP = cloTxt.AddComponent<TextMeshProUGUI>();
            cloTMP.text = "X"; cloTMP.fontSize = 18; cloTMP.fontStyle = FontStyles.Bold;
            cloTMP.alignment = TextAlignmentOptions.Center; cloTMP.color = Color.white;

            // Slots container
            var slots = UI("SlotsContainer", panel.transform);
            var slotsRT = slots.GetComponent<RectTransform>();
            slotsRT.anchorMin = Vector2.zero; slotsRT.anchorMax = Vector2.one;
            slotsRT.offsetMin = new Vector2(14,14); slotsRT.offsetMax = new Vector2(-14,-58);
            slots.AddComponent<Image>().color = new Color(0.07f,0.07f,0.12f,0.55f);
            var grid = slots.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(72,72); grid.spacing = new Vector2(8,8);
            grid.padding = new RectOffset(8,8,8,8);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 8;

            // Action menu
            var am = UI("ItemActionMenu", canvasRoot.transform);
            var amRT = am.GetComponent<RectTransform>();
            amRT.anchorMin = amRT.anchorMax = amRT.pivot = Vector2.zero; amRT.sizeDelta = new Vector2(160,215);
            var amScript = am.AddComponent<Script.Inventory.ItemActionMenu>();
            var mp = UI("MenuPanel", am.transform); Stretch(mp.GetComponent<RectTransform>());
            mp.AddComponent<Image>().color = new Color(0.10f,0.10f,0.16f,0.97f);
            var vl = mp.AddComponent<VerticalLayoutGroup>();
            vl.padding = new RectOffset(8,8,8,8); vl.spacing = 5;
            vl.childForceExpandWidth = true; vl.childForceExpandHeight = false; vl.childControlHeight = true;
            var inLbl = UI("ItemNameLabel", mp.transform); inLbl.AddComponent<LayoutElement>().minHeight = 28;
            var inTMP = inLbl.AddComponent<TextMeshProUGUI>();
            inTMP.text = "Item"; inTMP.fontSize = 13; inTMP.fontStyle = FontStyles.Bold;
            inTMP.alignment = TextAlignmentOptions.Center; inTMP.color = new Color(0.95f,0.85f,0.55f);
            var useBtn     = Btn("Use",     mp.transform, new Color(0.18f,0.45f,0.20f));
            var equipBtn   = Btn("Equip",   mp.transform, new Color(0.20f,0.28f,0.50f));
            var unequipBtn = Btn("Unequip", mp.transform, new Color(0.28f,0.20f,0.40f));
            var dropBtn    = Btn("Drop",    mp.transform, new Color(0.50f,0.18f,0.18f));
            var cloMenuBtn = Btn("Close",   mp.transform, new Color(0.22f,0.22f,0.28f));
            mp.SetActive(false);

            var amSO = new SerializedObject(amScript);
            amSO.FindProperty("menuPanel").objectReferenceValue    = mp;
            amSO.FindProperty("useButton").objectReferenceValue    = useBtn;
            amSO.FindProperty("equipButton").objectReferenceValue  = equipBtn;
            amSO.FindProperty("unequipButton").objectReferenceValue= unequipBtn;
            amSO.FindProperty("dropButton").objectReferenceValue   = dropBtn;
            amSO.FindProperty("closeButton").objectReferenceValue  = cloMenuBtn;
            amSO.FindProperty("itemNameText").objectReferenceValue = inTMP;
            amSO.ApplyModifiedProperties();

            // InventoryUI
            var invUI = root.AddComponent<Script.Inventory.InventoryUI>();
            var invSO = new SerializedObject(invUI);
            invSO.FindProperty("canvasRoot").objectReferenceValue     = canvasRoot;
            invSO.FindProperty("slotsContainer").objectReferenceValue = slots.transform;
            invSO.FindProperty("slotPrefab").objectReferenceValue     = slotPrefab;
            invSO.FindProperty("actionMenu").objectReferenceValue     = amScript;
            invSO.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/InventoryCanvas.prefab");
            Object.DestroyImmediate(root);
        }
    }
}