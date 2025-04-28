using Kchary.PhotoViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Kchary.PhotoViewer.Models
{

    /// <summary>
    /// サムネイル品質区分
    /// </summary>
    public enum ThumbnailQuality
    {
        Small,
        Medium,
        Full
    }

    /// <summary>
    /// サムネイルキャッシュ管理クラス
    /// </summary>
    public static class ThumbnailCache
    {
        private static readonly Dictionary<(string Path, ThumbnailQuality Quality), CacheEntry> thumbnailCache = [];
        private static readonly object thumbnailLock = new();

        private const long MaxCacheMemoryBytes = 500 * 1024 * 1024; // 500MB上限
        private static long currentCacheSize = 0;

        /// <summary>
        /// サムネイルをキャッシュから取得。なければ作成してキャッシュ。
        /// </summary>
        public static BitmapSource GetOrCreate(string filePath, ThumbnailQuality quality)
        {
            lock (thumbnailLock)
            {
                if (thumbnailCache.TryGetValue((filePath, quality), out var entry))
                {
                    entry.LastAccess = DateTime.UtcNow;
                    return entry.Image;
                }
            }

            var size = GetSizeForQuality(quality);

            var thumbnail = ImageUtil.GetThumbnail(filePath, size);
            if (thumbnail != null)
            {
                var sizeBytes = EstimateMemorySize(thumbnail);

                lock (thumbnailLock)
                {
                    if (currentCacheSize + sizeBytes > MaxCacheMemoryBytes)
                    {
                        TrimCache(sizeBytes);
                    }

                    thumbnailCache[(filePath, quality)] = new CacheEntry
                    {
                        Image = thumbnail,
                        Size = sizeBytes,
                        LastAccess = DateTime.UtcNow
                    };
                    currentCacheSize += sizeBytes;
                }
            }

            return thumbnail;
        }

        /// <summary>
        /// サムネイルを非同期で事前取得（プリフェッチ）
        /// </summary>
        public static void PrefetchThumbnails(IEnumerable<string> filePaths, ThumbnailQuality quality)
        {
            Task.Run(() =>
            {
                foreach (var path in filePaths)
                {
                    lock (thumbnailLock)
                    {
                        if (thumbnailCache.ContainsKey((path, quality)))
                        {
                            continue;
                        }
                    }

                    var thumb = ImageUtil.GetThumbnail(path, GetSizeForQuality(quality));
                    if (thumb != null)
                    {
                        var sizeBytes = EstimateMemorySize(thumb);

                        lock (thumbnailLock)
                        {
                            if (currentCacheSize + sizeBytes > MaxCacheMemoryBytes)
                            {
                                TrimCache(sizeBytes);
                            }

                            if (!thumbnailCache.ContainsKey((path, quality)))
                            {
                                thumbnailCache[(path, quality)] = new CacheEntry
                                {
                                    Image = thumb,
                                    Size = sizeBytes,
                                    LastAccess = DateTime.UtcNow
                                };
                                currentCacheSize += sizeBytes;
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// キャッシュ削除。古いものから必要分だけ削除する（LRU方式）
        /// </summary>
        private static void TrimCache(long requiredSpace)
        {
            var ordered = thumbnailCache.OrderBy(kvp => kvp.Value.LastAccess);
            long freed = 0;

            foreach (var kvp in ordered)
            {
                currentCacheSize -= kvp.Value.Size;
                freed += kvp.Value.Size;
                thumbnailCache.Remove(kvp.Key);

                if (freed >= requiredSpace)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// サムネイルサイズを推定する（Bytes単位）
        /// </summary>
        private static long EstimateMemorySize(BitmapSource bitmap)
        {
            return (long)bitmap.PixelWidth * bitmap.PixelHeight * (bitmap.Format.BitsPerPixel / 8);
        }

        /// <summary>
        /// 品質ごとのリサイズターゲット長さ
        /// </summary>
        private static int GetSizeForQuality(ThumbnailQuality quality)
        {
            return quality switch
            {
                ThumbnailQuality.Small => 100,
                ThumbnailQuality.Medium => 500,
                ThumbnailQuality.Full => 2000,
                _ => 100
            };
        }

        /// <summary>
        /// キャッシュエントリ情報
        /// </summary>
        private class CacheEntry
        {
            public BitmapSource Image { get; set; }
            public long Size { get; set; }
            public DateTime LastAccess { get; set; }
        }
    }
}