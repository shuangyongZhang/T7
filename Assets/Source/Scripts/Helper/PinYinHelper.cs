using System;
using System.Text;
using Microsoft.International.Converters.PinYinConverter;
using NPinyin;

namespace LB
{

    public class PinYinHelper
    {
        private static Encoding gb2312 = Encoding.GetEncoding("GB2312");
        /// <summary>
        /// 获取中文首拼音字符串
        /// </summary>
        /// <param name="chineseStr"></param>
        /// <returns></returns>
        public static string GetFirstChineseLetter(string chineseStr)
        {
            //中文编码

            //string pinYinFirstName = Pinyin.ConvertEncoding(chineseStr.Trim(), Encoding.UTF8, gb2312);
            //return Pinyin.GetInitials(pinYinFirstName);
            try
            {
                if (chineseStr.Length != 0)
                {
                    StringBuilder fullSpell = new StringBuilder();
                    for (int i = 0; i < chineseStr.Length; i++)
                    {
                        var chr = chineseStr[i];
                        fullSpell.Append(GetSpell(chr)[0]);
                    }

                    return fullSpell.ToString().ToUpper();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("首字母转化出错！" + e.Message);
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取中文全部拼音字符串
        /// </summary>
        /// <param name="chineseStr"></param>
        /// <returns></returns>
        public static string GetAllChineseLetter(string chineseStr)
        {
            string pinYinFullName = Pinyin.GetPinyin(chineseStr.Trim());
            return pinYinFullName;
        }

        /// <summary>
        /// 获取中文全部拼音字符串不带空格
        /// </summary>
        /// <param name="chineseStr"></param>
        /// <returns></returns>
        public static string GetAllChineseLetterWithoutSpace(string chineseStr)
        {
            string pinYinFullName = Pinyin.GetPinyin(chineseStr.Trim());
            pinYinFullName = pinYinFullName.Replace(" ", "");
            return pinYinFullName;
        }

        private static string GetSpell(char chr)
        {
            var coverchr = NPinyin.Pinyin.GetPinyin(chr);

            bool isChineses = ChineseChar.IsValidChar(coverchr[0]);
            if (isChineses)
            {
                ChineseChar chineseChar = new ChineseChar(coverchr[0]);
                foreach (string value in chineseChar.Pinyins)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value.Remove(value.Length - 1, 1);
                    }
                }
            }
            return coverchr;
        }
    }
}