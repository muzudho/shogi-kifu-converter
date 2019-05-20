﻿namespace Grayscale.ShogiKifuConverter
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO.Compression;
    using Grayscale.ShogiKifuConverter.CommonAction;
    using Grayscale.ShogiKifuConverter.Commons;
    using Grayscale.ShogiKifuConverter.Location;

    /// <summary>
    /// Zip圧縮ファイル。
    /// </summary>
    public class ZipArchiveFile : AbstractArchiveFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZipArchiveFile"/> class.
        /// </summary>
        /// <param name="expansionGoFile">解凍を待っているファイル。</param>
        public ZipArchiveFile(TraceableFile expansionGoFile)
            : base(expansionGoFile)
        {
            // Trace.WriteLine($"Zip: {this.ExpansionGoFilePath}");
        }

        /// <summary>
        /// 解凍する。
        /// </summary>
        /// <returns>展開に成功した。</returns>
        public override bool Expand()
        {
            Trace.WriteLine($"{LogHelper.Stamp}Expand  : {this.ExpansionGoFile.FullName} -> {LocationMaster.ConverterExpandDirectory.FullName}");

            /*
            // 既存なら削除。
            var file = new TraceableFile(LocationMaster.ConverterExpandDirectory.FullName);
            file.Delete();
            */

            ZipFile.ExtractToDirectory(this.ExpansionGoFile.FullName, LocationMaster.ConverterExpandDirectory.FullName);

            // ディレクトリーを浅くします。
            PathFlat.GoFlat(LocationMaster.ConverterExpandDirectory.FullName);

            // 解凍が終わった元ファイルは削除。
            this.ExpansionGoFile.Delete();

            // 解凍したとき作ったディレクトリーが残ってしまう。ディレクトリーは消す。
            {
                IEnumerable<string> restDirectories =
                    System.IO.Directory.EnumerateDirectories(
                        LocationMaster.ConverterExpandDirectory.FullName, "*", System.IO.SearchOption.TopDirectoryOnly);

                foreach (var restDir in restDirectories)
                {
                    new TraceableDirectory(restDir).Delete(true);
                }
            }

            return true;
        }
    }
}
