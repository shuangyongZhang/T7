using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LB
{
    /// <summary>
    /// 页码条：上一页/下一页 + 页码按钮；页码过多时保留首末页与当前页±1，间隔用 ... 填充
    /// </summary>
    public static class UIPagerBar
    {
        private const float SlotGap = 4f;
        private const float FlankGap = 8f;
        private static readonly Color NormalBg = new Color(0.941f, 0.941f, 0.941f, 1f);
        private static readonly Color NormalText = new Color(0.133f, 0.133f, 0.133f, 1f);
        private static readonly Color ActiveBg = new Color(0.094f, 0.118f, 0.145f, 1f);
        private static readonly Color DisabledText = new Color(0.557f, 0.557f, 0.576f, 1f);

        public static void Refresh(RectTransform pager, Button prev, Button next,
            GameObject numberTemplate, GameObject ellipsisTemplate,
            int pageCount, int currentPage, Action<int> onPageSelected)
        {
            for (int i = pager.childCount - 1; i >= 0; i--)
            {
                Transform child = pager.GetChild(i);
                if (child == numberTemplate.transform || child == ellipsisTemplate.transform ||
                    child == prev.transform || child == next.transform) continue;
                child.gameObject.SetActive(false);
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }

            List<int> slots = BuildSlots(pageCount, currentPage);
            float numberWidth = numberTemplate.GetComponent<RectTransform>().sizeDelta.x;
            float ellipsisWidth = ellipsisTemplate.GetComponent<RectTransform>().sizeDelta.x;
            float total = 0f;
            for (int i = 0; i < slots.Count; i++)
                total += (slots[i] < 0 ? ellipsisWidth : numberWidth) + (i > 0 ? SlotGap : 0f);

            float x = -total / 2f;
            for (int i = 0; i < slots.Count; i++)
            {
                bool isEllipsis = slots[i] < 0;
                float w = isEllipsis ? ellipsisWidth : numberWidth;
                GameObject node = UnityEngine.Object.Instantiate(isEllipsis ? ellipsisTemplate : numberTemplate, pager);
                node.name = isEllipsis ? "PageEllipsis_" + i : "PageNumber_" + slots[i];
                node.SetActive(true);
                node.GetComponent<RectTransform>().anchoredPosition = new Vector2(x + w / 2f, 0f);
                if (!isEllipsis)
                {
                    int page = slots[i];
                    bool active = page == currentPage;
                    Image bg = node.GetComponent<Image>();
                    if (bg != null) bg.color = active ? ActiveBg : NormalBg;
                    TMP_Text label = node.GetComponentInChildren<TMP_Text>();
                    if (label != null)
                    {
                        label.text = (page + 1).ToString();
                        label.color = active ? Color.white : NormalText;
                    }
                    Button button = node.GetComponent<Button>();
                    button.transition = Selectable.Transition.None;
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => onPageSelected(page));
                }
                x += w + SlotGap;
            }

            float prevWidth = prev.GetComponent<RectTransform>().sizeDelta.x;
            float nextWidth = next.GetComponent<RectTransform>().sizeDelta.x;
            prev.GetComponent<RectTransform>().anchoredPosition = new Vector2(-total / 2f - FlankGap - prevWidth / 2f, 0f);
            next.GetComponent<RectTransform>().anchoredPosition = new Vector2(total / 2f + FlankGap + nextWidth / 2f, 0f);
            SetFlank(prev, currentPage > 0);
            SetFlank(next, currentPage < pageCount - 1);
        }

        private static void SetFlank(Button button, bool enabled)
        {
            button.transition = Selectable.Transition.None;
            button.interactable = enabled;
            Image bg = button.GetComponent<Image>();
            if (bg != null) bg.color = NormalBg;
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null) label.color = enabled ? NormalText : DisabledText;
        }

        private static List<int> BuildSlots(int pageCount, int currentPage)
        {
            var slots = new List<int>();
            if (pageCount <= 7)
            {
                for (int p = 0; p < pageCount; p++) slots.Add(p);
                return slots;
            }

            var keep = new List<int> { 0, pageCount - 1, currentPage - 1, currentPage, currentPage + 1 };
            keep.RemoveAll(p => p < 0 || p >= pageCount);
            keep.Sort();
            int prev = -1;
            foreach (int p in keep)
            {
                if (prev >= 0 && p - prev > 1) slots.Add(-1);
                if (p != prev) slots.Add(p);
                prev = p;
            }
            return slots;
        }
    }
}