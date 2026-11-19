using System;
using System.Collections.Generic;
using hyjiacan.py4n;
using UnityEngine;

namespace PrabdeepDhaliwal.OnScreenKeyboard
{
    /// <summary>
    /// 基于 Pinyin4Net 的拼音输入处理器（无后备候选词，支持分页）
    /// </summary>
    public class PinyinProcessor
    {
        private string pinyinBuffer = "";
        private List<string> allCandidates = new List<string>();   // 所有匹配的候选词
        private int currentPage = 0;
        private readonly int pageSize;          // 每页显示数量

        public string PinyinBuffer => pinyinBuffer;
        public IReadOnlyList<string> AllCandidates => allCandidates;
        public bool HasPendingInput => !string.IsNullOrEmpty(pinyinBuffer);
        public int TotalCandidateCount => allCandidates.Count;
        public int CurrentPage => currentPage;
        public int PageSize => pageSize;
        public int TotalPages => Mathf.CeilToInt((float)TotalCandidateCount / pageSize);

        public PinyinProcessor(int pageSize = 5)
        {
            this.pageSize = pageSize;
        }

        /// <summary>
        /// 输入一个字母（追加到拼音缓冲区）
        /// </summary>
        public void InputLetter(char c)
        {
            if (!char.IsLetter(c)) return;
            pinyinBuffer += char.ToLower(c);
            RefreshCandidates();
        }

        /// <summary>
        /// 退格删除最后一个拼音字符
        /// </summary>
        public void Backspace()
        {
            if (pinyinBuffer.Length > 0)
            {
                pinyinBuffer = pinyinBuffer.Remove(pinyinBuffer.Length - 1);
                RefreshCandidates();
            }
        }

        /// <summary>
        /// 清空所有状态（拼音缓冲区、候选词、页码）
        /// </summary>
        public void Clear()
        {
            pinyinBuffer = "";
            allCandidates.Clear();
            currentPage = 0;
        }

        /// <summary>
        /// 使用 Pinyin4Net 刷新候选词列表（精确匹配完整拼音）
        /// </summary>
        private void RefreshCandidates()
        {
            allCandidates.Clear();
            currentPage = 0;
            if (string.IsNullOrEmpty(pinyinBuffer)) return;

            try
            {
                // 直接传入完整拼音，获取所有读音完全匹配的汉字
                string[] hanziArray = Pinyin4Net.GetHanzi(pinyinBuffer, false);
                if (hanziArray != null && hanziArray.Length > 0)
                {
                    allCandidates.AddRange(hanziArray);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Pinyin4Net 查询失败: {ex.Message}");
                allCandidates.Clear();
            }
        }

        /// <summary>
        /// 获取当前页的候选词列表（用于显示）
        /// </summary>
        public List<string> GetCurrentPageCandidates()
        {
            if (allCandidates.Count == 0) return new List<string>();

            int start = currentPage * pageSize;
            if (start >= allCandidates.Count) return new List<string>();

            int end = Math.Min(start + pageSize, allCandidates.Count);
            return allCandidates.GetRange(start, end - start);
        }

        /// <summary>
        /// 翻到下一页
        /// </summary>
        public bool NextPage()
        {
            if (currentPage < TotalPages - 1)
            {
                currentPage++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 翻到上一页
        /// </summary>
        public bool PreviousPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 根据当前页的相对索引获取候选词（0 ~ pageSize-1）
        /// </summary>
        public string GetCandidateAtPageIndex(int pageIndex)
        {
            var pageList = GetCurrentPageCandidates();
            if (pageIndex < 0 || pageIndex >= pageList.Count) return null;
            return pageList[pageIndex];
        }

        /// <summary>
        /// 确认输入：返回当前页第一个候选词，若没有则返回拼音本身
        /// </summary>
        public string Confirm(out string selected)
        {
            var pageCandidates = GetCurrentPageCandidates();
            selected = pageCandidates.Count > 0 ? pageCandidates[0] : pinyinBuffer;
            Clear();
            return selected;
        }
    }
}