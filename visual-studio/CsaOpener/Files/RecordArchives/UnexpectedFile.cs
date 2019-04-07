﻿namespace Grayscale.CsaOpener
{
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// 予期しない形式のファイル。
    /// </summary>
    public class UnexpectedFile : AbstractArchiveFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedFile"/> class.
        /// </summary>
        /// <param name="config">設定。</param>
        /// <param name="expansionGoFilePath">解凍を待っているファイルパス。</param>
        public UnexpectedFile(KifuwarabeWcsc29Config config, string expansionGoFilePath)
            : base(config, expansionGoFilePath)
        {
            Trace.WriteLine($"Unexpected file: {this.ExpansionGoFilePath}");

            // .exe とか解凍できないやつが入っている☆（＾～＾）！
            Trace.WriteLine($"むり: {this.ExpansionGoFilePath}");

            // そのままコピーすると名前がぶつかってしまう☆（＾～＾）
            this.ExpansionOutputDir = Path.Combine(config.ExpansionOutputPath, $"copied-{Path.GetFileNameWithoutExtension(this.ExpansionGoFilePath)}");
            Commons.CreateDirectory(this.ExpansionOutputDir);
        }

        /// <summary>
        /// 解凍する。
        /// </summary>
        public override void Expand()
        {
            if (string.IsNullOrWhiteSpace(this.ExpansionGoFilePath))
            {
                return;
            }

            var wentDir = Path.Combine(this.Config.ExpansionWentPath, Path.GetFileName(this.ExpansionGoFilePath));
            Trace.WriteLine($"Evasion: {this.ExpansionGoFilePath} -> {wentDir}");

            // 無理だった元ファイルを移動。
            File.Move(this.ExpansionGoFilePath, wentDir);
        }
    }
}