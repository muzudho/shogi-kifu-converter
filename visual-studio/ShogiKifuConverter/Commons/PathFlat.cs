﻿namespace Grayscale.ShogiKifuConverter.CommonAction
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using Grayscale.ShogiKifuConverter.Commons;

    /// <summary>
    /// ２つの圧縮ファイルがある場面を考えてみて欲しい。
    ///
    /// C:\This\Is\A\Cat.zip
    /// C:\This\Is\A\Dog.zip
    ///
    /// この２つの圧縮ファイルの中のディレクトリ構成はまだ分からない。この２つを解凍し、中身を１つのディレクトリーにまとめる。
    /// 名前被りで衝突することが考えられる。
    ///
    /// C:\This\Is\A\Cat\The\Very\Fat.txt
    /// C:\This\Is\A\Dog\The\Very\Fat.txt
    ///
    /// そこで解凍したファイルは、以下のようにリネームすることにした。
    ///
    /// C:\This\Is\A\Cat\Cat$%The$%Very$%Fat.txt
    /// C:\This\Is\A\Dog\Dog$%The$%Very$%Fat.txt
    ///
    /// 適当に決めた '$%' で、パス区切り文字を消すことで、ディレクトリーの下をファイルのみ（フラット）にする。
    ///
    /// 以下、コード中で、CatやDogディレクトリーのことを 傘ディレクトリー、
    /// Veryディレクトリーのことを 中間ノード と呼ぶ。
    ///
    /// 傘ディレクトリーは残しつつ、ファイル名の先頭に 傘ディレクトリー名が含まれていることが工夫点だ。
    /// この工夫のせいで 名前被りが発生するケースは 運用上 避けているものとしてよい。
    /// </summary>
    public class PathFlat
    {
        /// <summary>
        /// ディレクトリーの中を全部実行。
        /// </summary>
        /// <param name="directoryLikeUmbrella">このディレクトリーの下をフラットにする。このディレクトリー自身は残す。</param>
        public static void GoFlat(string directoryLikeUmbrella)
        {
            Trace.WriteLine($"{LogHelper.Stamp}GoFlat  : {directoryLikeUmbrella}.");

            // 傘の下の中間ディレクトリーたち。こいつらが消える。
            IEnumerable<string> intermediateDirectories =
                System.IO.Directory.EnumerateDirectories(
                    directoryLikeUmbrella, "*", System.IO.SearchOption.TopDirectoryOnly);

            foreach (var intermediateDir in intermediateDirectories)
            {
                // 再帰呼出し。
                PathFlat.VisitIntermadiateDirectory(intermediateDir);
            }

            // 傘の直下のファイルは既にフラットなのでやる必要なし。
            // ここまでで、フラットになっている。
            /*
            // ファイルの頭に、傘を付けていく。
            Trace.WriteLine($"{LogHelper.Stamp}GoRename: {directoryLikeUmbrella}.");
            IEnumerable<string> fileNames =
                System.IO.Directory.EnumerateFiles(
                    directoryLikeUmbrella, "*", System.IO.SearchOption.TopDirectoryOnly);
            foreach (var fileName in fileNames)
            {
                // 親ディレクトリーの下に、親ディレクトリーの名前と、主体のファイル名を $% でくっつけたものを置く。
                // Ｃ＃のメソッド名は、ノード名なのか、フルパスなのか、はっきりわかるように名付けてほしい。最後のノード名は、ファイル名を取るメソッドで代用した。
                var joinedName = $"{directoryLikeUmbrella}\\{Path.GetFileName(directoryLikeUmbrella)}$%{Path.GetFileName(fileName)}";

                // リネーム☆ 衝突したら上書き。
                new TraceableFile(fileName).Move(new TraceableFile(joinedName), true);
            }
            */
        }

        /// <summary>
        /// 再帰サーチ。
        /// </summary>
        /// <param name="intermediateDir">消えていく中間ディレクトリー。</param>
        private static void VisitIntermadiateDirectory(string intermediateDir)
        {
            // もっと中間ディレクトリーたち。
            IEnumerable<string> moreIntermediateDirectories =
                System.IO.Directory.EnumerateDirectories(
                    intermediateDir, "*", System.IO.SearchOption.TopDirectoryOnly);

            foreach (var moreIntermediateDir in moreIntermediateDirectories)
            {
                // 再帰呼出し。
                PathFlat.VisitIntermadiateDirectory(moreIntermediateDir);
            }

            // この階層のファイル。これを残す。
            IEnumerable<string> files =
                System.IO.Directory.EnumerateFiles(
                    intermediateDir, "*", System.IO.SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                PathFlat.DeleteParentDirectoryByMerge(new TraceableFile(file));
            }
        }

        /// <summary>
        /// 親ディレクトリーの名前と、主体のファイル名を $% でくっつける。
        /// </summary>
        /// <param name="file">ファイル名。</param>
        private static void DeleteParentDirectoryByMerge(TraceableFile file)
        {
            // 親ディレクトリーの名前と、主体のファイル名を $% でくっつける。
            var joinedName = $"{Directory.GetParent(file.FullName).Name}$%{Path.GetFileName(file.FullName)}";

            // 親の親ディレクトリーのフル名。（なければ例外）
            var parentParentDirectory = Directory.GetParent(Directory.GetParent(file.FullName).FullName).FullName;

            // 親の親ディレクトリーの下に さっきくっつけた名前。衝突したら Move が例外を投げる。
            Trace.WriteLine($"{LogHelper.Stamp}parentParentDirectory: '{parentParentDirectory}', joinedName: '{joinedName}'.");
            var destination = new TraceableFile(PathHelper.Combine(parentParentDirectory, joinedName));

            // 古い名前から、新しい名前へ移動。
            file.MoveTo(destination, true);
        }
    }
}
