﻿namespace Kchary.PhotoViewer.Models
{
    /// <summary>
    /// 編集画面に表示する画質設定メニュークラス
    /// </summary>
    public sealed record ImageQuality
    {
        /// <summary>
        /// 画質名
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// 画質に対応する値
        /// </summary>
        public int QualityValue { get; init; }
    }
}