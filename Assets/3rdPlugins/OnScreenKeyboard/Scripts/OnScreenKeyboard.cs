using HT.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrabdeepDhaliwal.OnScreenKeyboard
{
    public enum InputMode
    {
        English,
        Chinese
    }

    public class OnScreenKeyboard : HTBehaviour
    {
        protected override bool IsAutomate => true;

        #region Singleton
        public static OnScreenKeyboard Instance;
        protected override void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        protected override void OnDestroy() { if (Instance == this) Instance = null; }
        #endregion

        #region Fields
        [Header("Basic Keys")]
        public Key[] keys;

        [Space]
        [Header("Special Keys")]
        public Button shiftButton;
        public Button capsLockButton;
        public Button enterButton;
        public Button backspaceButton;
        public Button deleteButton;
        public Button spaceButton;
        public Button leftArrowButton;
        public Button rightArrowButton;

        [Space]
        [Header("Chinese Input")]
        public Button languageToggleButton;
        public TextMeshProUGUI languageModeText;
        public GameObject pinyinPanel;
        public TextMeshProUGUI pinyinText;
        public Button[] candidateButtons;
        public Button prevPageButton;
        public Button nextPageButton;
        public TextMeshProUGUI pageText;

        public List<string> ignoreKeyNames = new List<string> { "Shift", "Caps", "Enter", "Delete", "Back", "Space", "LeftArrow", "RightArrow" };

        private bool isCapsLockOn;
        private bool isShiftPressed;

        // 双输入字段支持
        private TMP_InputField tmpInputField;
        private InputField legacyInputField;

        private InputMode currentInputMode = InputMode.English;
        private PinyinProcessor pinyinProcessor;

        private int PageSize => candidateButtons?.Length ?? 5;
        #endregion

        #region Initialization
        private void Start()
        {
            if (pinyinProcessor == null)
                pinyinProcessor = new PinyinProcessor(PageSize);

            foreach (var key in keys)
            {
                key.Setup();
                key.button.onClick.AddListener(() => OnKeyPress(key));
            }

            AddButtonListeners();
            InitializeChineseInput();
            // ToggleKeyboardVisibility();
        }

        private void OnEnable()
        {
            if (pinyinProcessor == null)
                pinyinProcessor = new PinyinProcessor(PageSize);
            ResetKeyState();
        }
        private void OnDisable()
        {
            tmpInputField = null;
            legacyInputField = null;
            pinyinProcessor?.Clear();
            UpdatePinyinDisplay();
            UpdateCandidateDisplay();
        }

        public void Setup(TMP_InputField inputField)
        {
            tmpInputField = inputField;
            legacyInputField = null;
            ResetKeyState();
        }

        public void Setup(InputField inputField)
        {
            legacyInputField = inputField;
            tmpInputField = null;
            ResetKeyState();
        }

        private void InitializeChineseInput()
        {
            if (languageToggleButton != null)
                languageToggleButton.onClick.AddListener(ToggleInputMode);
            UpdateLanguageModeDisplay();
            if (pinyinPanel != null) pinyinPanel.SetActive(false);

            if (prevPageButton != null) prevPageButton.onClick.AddListener(ShowPreviousPage);
            if (nextPageButton != null) nextPageButton.onClick.AddListener(ShowNextPage);

            if (candidateButtons != null)
            {
                for (int i = 0; i < candidateButtons.Length; i++)
                {
                    int idx = i;
                    candidateButtons[i].onClick.AddListener(() => SelectCandidate(idx));
                }
            }
            UpdateCandidateDisplay();
        }

        private void AddButtonListeners()
        {
            capsLockButton?.onClick.AddListener(ToggleCapsLock);
            shiftButton?.onClick.AddListener(() => ToggleShift());
            enterButton?.onClick.AddListener(SubmitText);
            backspaceButton?.onClick.AddListener(DeleteCharacter);
            deleteButton?.onClick.AddListener(DeleteCharacterAtCaret);
            spaceButton?.onClick.AddListener(InsertSpaceCharacter);
            leftArrowButton?.onClick.AddListener(MoveCaretLeft);
            rightArrowButton?.onClick.AddListener(MoveCaretRight);
        }

        private void RemoveButtonListeners()
        {
            capsLockButton?.onClick.RemoveAllListeners();
            shiftButton?.onClick.RemoveAllListeners();
            enterButton?.onClick.RemoveAllListeners();
            backspaceButton?.onClick.RemoveAllListeners();
            deleteButton?.onClick.RemoveAllListeners();
            spaceButton?.onClick.RemoveAllListeners();
            leftArrowButton?.onClick.RemoveAllListeners();
            rightArrowButton?.onClick.RemoveAllListeners();
            foreach (var key in keys) key.button.onClick.RemoveAllListeners();
            languageToggleButton?.onClick.RemoveListener(ToggleInputMode);
            prevPageButton?.onClick.RemoveListener(ShowPreviousPage);
            nextPageButton?.onClick.RemoveListener(ShowNextPage);
            if (candidateButtons != null)
                foreach (var btn in candidateButtons) btn.onClick.RemoveAllListeners();
        }
        #endregion

        #region Key Press Handling
        private void OnKeyPress(Key key)
        {
            string keyText = GetKeyText(key);
            if (currentInputMode == InputMode.Chinese)
                HandleChineseKeyPress(keyText);
            else
                InsertCharacter(keyText);
            if (isShiftPressed) ToggleShift();
        }

        private string GetKeyText(Key key)
        {
            if (isShiftPressed)
            {
                if (string.IsNullOrEmpty(key.secondaryValue))
                {
                    return key.primaryValue.Length == 1 && char.IsLetter(key.primaryValue[0])
                        ? (isCapsLockOn ? key.primaryValue.ToLower() : key.primaryValue.ToUpper())
                        : key.primaryValue;
                }
                return key.secondaryValue;
            }
            else
            {
                return isCapsLockOn
                    ? key.primaryValue.ToUpper()
                    : key.primaryValue.ToLower();
            }
        }

        private void ToggleCapsLock() { isShiftPressed = false; isCapsLockOn = !isCapsLockOn; UpdateKeyLabels(); }
        private void ToggleShift() { isShiftPressed = !isShiftPressed; UpdateKeyLabels(); }

        private void UpdateKeyLabels()
        {
            foreach (var key in keys)
            {
                if (key.primaryKeyText == null || key.secondaryKeyText == null || ignoreKeyNames.Contains(key.primaryValue))
                    continue;
                if (isShiftPressed)
                    UpdateKeyTextForShift(key);
                else
                    UpdateKeyTextForNormal(key);
            }
        }

        private void UpdateKeyTextForShift(Key key)
        {
            if (string.IsNullOrEmpty(key.secondaryValue))
            {
                key.primaryKeyText.text = key.primaryValue.Length == 1 && char.IsLetter(key.primaryValue[0])
                    ? (isCapsLockOn ? key.primaryValue.ToLower() : key.primaryValue.ToUpper())
                    : key.primaryValue;
            }
            else
            {
                SwapKeyTexts(key);
            }
        }

        private void UpdateKeyTextForNormal(Key key)
        {
            if (isCapsLockOn)
            {
                if (key.primaryValue.Length == 1 && char.IsLetter(key.primaryValue[0]))
                    key.primaryKeyText.text = key.primaryValue.ToUpper();
                else
                    key.primaryKeyText.text = key.primaryValue;
            }
            else
            {
                key.primaryKeyText.text = key.primaryValue.Length == 1 && char.IsLetter(key.primaryValue[0])
                    ? key.primaryValue.ToLower()
                    : key.primaryValue;
            }
            key.secondaryKeyText.text = key.secondaryValue;
        }

        private void SwapKeyTexts(Key key)
        {
            var temp = key.primaryKeyText.text;
            key.primaryKeyText.text = key.secondaryValue;
            key.secondaryKeyText.text = temp;
        }
        #endregion

        #region Character Insertion and Deletion（兼容两种输入）
        private bool IsInputValid() => tmpInputField != null || legacyInputField != null;

        private void InsertCharacter(string character)
        {
            if (string.IsNullOrEmpty(character) || !IsInputValid()) return;

            if (tmpInputField != null)
            {
                // 1. 处理可能的选中文本
                int pos = tmpInputField.caretPosition;
                if (tmpInputField.selectionStringAnchorPosition != tmpInputField.selectionStringFocusPosition)
                {
                    int start = Mathf.Min(tmpInputField.selectionStringAnchorPosition, tmpInputField.selectionStringFocusPosition);
                    int end = Mathf.Max(tmpInputField.selectionStringAnchorPosition, tmpInputField.selectionStringFocusPosition);
                    tmpInputField.text = tmpInputField.text.Remove(start, end - start);
                    pos = start;
                }

                // 2. 插入新字符
                tmpInputField.text = tmpInputField.text.Insert(pos, character);
                pos += character.Length;

                // 3. 强制更新文本标签
                tmpInputField.ForceLabelUpdate();

                // 4. 设置光标位置并清除任何选中状态
                tmpInputField.caretPosition = pos;
                tmpInputField.selectionStringAnchorPosition = pos;
                tmpInputField.selectionStringFocusPosition = pos;

                // 5. 重新激活输入字段以刷新内部状态
                tmpInputField.ActivateInputField();
            }
            else if (legacyInputField != null)
            {
                // 旧版 InputField 简化处理（仅使用 caretPosition）
                int pos = legacyInputField.caretPosition;
                legacyInputField.text = legacyInputField.text.Insert(pos, character);
                pos += character.Length;
                legacyInputField.caretPosition = pos;
            }
        }

        private void InsertSpaceCharacter()
        {
            if (currentInputMode == InputMode.Chinese && pinyinProcessor.HasPendingInput)
            {
                ConfirmPinyinInput();
                return;
            }
            InsertCharacter(" ");
        }

        private void DeleteCharacter()
        {
            if (!IsInputValid()) return;

            // 中文拼音删除
            if (currentInputMode == InputMode.Chinese && pinyinProcessor.HasPendingInput)
            {
                pinyinProcessor.Backspace();
                UpdatePinyinDisplay();
                UpdateCandidateDisplay();
                return;
            }

            // TMP 完整功能
            if (tmpInputField != null)
            {
                int start = Mathf.Min(tmpInputField.selectionStringAnchorPosition, tmpInputField.selectionStringFocusPosition);
                int end = Mathf.Max(tmpInputField.selectionStringAnchorPosition, tmpInputField.selectionStringFocusPosition);
                if (start != end)
                {
                    tmpInputField.text = tmpInputField.text.Remove(start, end - start);
                    tmpInputField.caretPosition = start;
                }
                else if (tmpInputField.caretPosition > 0)
                {
                    int pos = tmpInputField.caretPosition;
                    tmpInputField.text = tmpInputField.text.Remove(pos - 1, 1);
                    tmpInputField.caretPosition = pos - 1;
                }
                return;
            }

            // 旧版简化：删除光标前一个字符
            if (legacyInputField != null)
            {
                if (legacyInputField.caretPosition > 0)
                {
                    int pos = legacyInputField.caretPosition;
                    legacyInputField.text = legacyInputField.text.Remove(pos - 1, 1);
                    legacyInputField.caretPosition = pos - 1;
                }
            }
        }

        private void DeleteCharacterAtCaret()
        {
            if (!IsInputValid()) return;

            if (tmpInputField != null)
            {
                int start = Mathf.Min(tmpInputField.selectionStringAnchorPosition, tmpInputField.selectionStringFocusPosition);
                int end = Mathf.Max(tmpInputField.selectionStringAnchorPosition, tmpInputField.selectionStringFocusPosition);
                if (start != end)
                {
                    tmpInputField.text = tmpInputField.text.Remove(start, end - start);
                    tmpInputField.caretPosition = start;
                }
                else if (tmpInputField.caretPosition < tmpInputField.text.Length)
                {
                    tmpInputField.text = tmpInputField.text.Remove(tmpInputField.caretPosition, 1);
                }
                return;
            }

            if (legacyInputField != null)
            {
                if (legacyInputField.caretPosition < legacyInputField.text.Length)
                {
                    int pos = legacyInputField.caretPosition;
                    legacyInputField.text = legacyInputField.text.Remove(pos, 1);
                }
            }
        }
        #endregion

        #region Caret Movement
        private void MoveCaretLeft() => MoveCaret(-1);
        private void MoveCaretRight() => MoveCaret(1);

        private void MoveCaret(int direction)
        {
            if (!IsInputValid()) return;

            if (tmpInputField != null)
            {
                int pos = tmpInputField.selectionStringAnchorPosition != tmpInputField.selectionStringFocusPosition
                    ? Mathf.Min(tmpInputField.selectionStringAnchorPosition, tmpInputField.selectionStringFocusPosition)
                    : Mathf.Clamp(tmpInputField.caretPosition + direction, 0, tmpInputField.text.Length);
                tmpInputField.caretPosition = pos;
                return;
            }

            if (legacyInputField != null)
            {
                int pos = Mathf.Clamp(legacyInputField.caretPosition + direction, 0, legacyInputField.text.Length);
                legacyInputField.caretPosition = pos;
            }
        }
        #endregion

        #region Submit & Visibility
        private void SubmitText()
        {
            if (currentInputMode == InputMode.Chinese && pinyinProcessor.HasPendingInput)
            {
                ConfirmPinyinInput();
                return;
            }

            // 模拟失去焦点（兼容两种）
            if (tmpInputField != null && tmpInputField.TryGetComponent(out ManagedInputField managedTmp))
                managedTmp.Deselected();
            else if (legacyInputField != null && legacyInputField.TryGetComponent(out ManagedInputField managedLegacy))
                managedLegacy.Deselected();
        }

        //public void ToggleKeyboardVisibility() => gameObject.SetActive(!gameObject.activeSelf);

        private void ResetKeyState()
        {
            isCapsLockOn = false;
            isShiftPressed = false;
            UpdateKeyLabels();
            pinyinProcessor?.Clear();
            UpdatePinyinDisplay();
            UpdateCandidateDisplay();
        }
        #endregion

        #region Chinese Input Processing
        private void HandleChineseKeyPress(string keyText)
        {
            if (keyText.Length == 1 && char.IsLetter(keyText[0]))
            {
                pinyinProcessor.InputLetter(keyText[0]);
                UpdatePinyinDisplay();
                UpdateCandidateDisplay();
            }
            else
            {
                if (pinyinProcessor.HasPendingInput)
                {
                    string confirmed = pinyinProcessor.Confirm(out _);
                    if (!string.IsNullOrEmpty(confirmed))
                        InsertCharacter(confirmed);
                    UpdatePinyinDisplay();
                    UpdateCandidateDisplay();
                }
                InsertCharacter(keyText);
            }
        }

        private void ConfirmPinyinInput()
        {
            if (!pinyinProcessor.HasPendingInput) return;
            string confirmed = pinyinProcessor.Confirm(out _);
            if (!string.IsNullOrEmpty(confirmed))
                InsertCharacter(confirmed);
            UpdatePinyinDisplay();
            UpdateCandidateDisplay();
        }

        private void ClearPinyinBuffer()
        {
            pinyinProcessor.Clear();
            UpdatePinyinDisplay();
            UpdateCandidateDisplay();
        }

        private void UpdatePinyinDisplay()
        {
            if (pinyinPanel != null)
                pinyinPanel.SetActive(pinyinProcessor.HasPendingInput);
            if (pinyinText != null)
                pinyinText.text = pinyinProcessor.PinyinBuffer;
        }

        private void UpdateCandidateDisplay()
        {
            if (candidateButtons == null) return;

            var pageCandidates = pinyinProcessor.GetCurrentPageCandidates();
            int pageSize = candidateButtons.Length;

            for (int i = 0; i < pageSize; i++)
            {
                if (i < pageCandidates.Count)
                {
                    candidateButtons[i].gameObject.SetActive(true);
                    Text text = candidateButtons[i].GetComponentInChildren<Text>();
                    if (text != null) text.text = pageCandidates[i];
                }
                else
                {
                    candidateButtons[i].gameObject.SetActive(false);
                }
            }

            if (pageText != null)
            {
                int total = pinyinProcessor.TotalPages;
                int current = pinyinProcessor.CurrentPage + 1;
                pageText.text = total > 0 ? $"{current}/{total}" : "";
            }
            if (prevPageButton != null)
                prevPageButton.interactable = pinyinProcessor.CurrentPage > 0;
            if (nextPageButton != null)
                nextPageButton.interactable = pinyinProcessor.CurrentPage < pinyinProcessor.TotalPages - 1;
        }

        public void SelectCandidate(int pageIndex)
        {
            string candidate = pinyinProcessor.GetCandidateAtPageIndex(pageIndex);
            if (!string.IsNullOrEmpty(candidate))
            {
                InsertCharacter(candidate);
                pinyinProcessor.Clear();
                UpdatePinyinDisplay();
                UpdateCandidateDisplay();

                // 重新聚焦（两种）
                if (tmpInputField != null)
                    tmpInputField.ActivateInputField();
                else if (legacyInputField != null)
                    legacyInputField.ActivateInputField();
            }
        }

        private void ShowPreviousPage()
        {
            if (pinyinProcessor.PreviousPage())
                UpdateCandidateDisplay();
        }

        private void ShowNextPage()
        {
            if (pinyinProcessor.NextPage())
                UpdateCandidateDisplay();
        }

        public void ToggleInputMode()
        {
            currentInputMode = currentInputMode == InputMode.English ? InputMode.Chinese : InputMode.English;
            UpdateLanguageModeDisplay();
            if (currentInputMode == InputMode.English)
                ClearPinyinBuffer();
        }

        private void UpdateLanguageModeDisplay()
        {
            if (languageModeText != null)
                languageModeText.text = currentInputMode == InputMode.English ? "EN" : "中";
        }

        public InputMode GetCurrentInputMode() => currentInputMode;
        public void SetInputMode(InputMode mode)
        {
            currentInputMode = mode;
            UpdateLanguageModeDisplay();
            if (mode == InputMode.English)
                ClearPinyinBuffer();
        }
        #endregion

        #region Keyboard Input (方向键翻页)
        private void Update()
        {
            if (!isActiveAndEnabled || !pinyinProcessor.HasPendingInput) return;

            if (Input.GetKeyDown(KeyCode.LeftArrow))
                ShowPreviousPage();
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                ShowNextPage();
        }
        #endregion
    }
}