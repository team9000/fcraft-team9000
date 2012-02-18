/*
 *  Copyright 2009, 2010, 2011 Matvei Stefarov <me@matvei.org>
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *  THE SOFTWARE.
 *
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using fCraft;
using fCraft.Events;
using fCraft.GUI;


namespace fCraftImg {

    static class Program {
        static void Main( string[] args ) {
            if (args.Length < 2) {
                Console.WriteLine("Usage: fCraftImg.exe <map filename> <output png> [rotation (0-3)] [mode] [x y z x2 y2 z2]");
                return;
            }

            int i = 0;
            string filename = args[i++];
            string output = args[i++];
            int rotation = 0;
            if (args.Length >= i+1) rotation = System.Convert.ToInt32(args[i++]);
            int mode = 0;
            if (args.Length >= i+1) mode = System.Convert.ToInt32(args[i++]);

            int[] chunkCoords = new int[6];
            for (int pos = 0; pos < 6; pos++) {
                if (args.Length < i + 1) break;
                chunkCoords[pos] = System.Convert.ToInt32(args[i++]);
            }

            try {
                Map map = fCraft.MapConversion.MapUtility.Load(filename);
                map.CalculateShadows();
                fCraft.GUI.IsoCat renderer = new fCraft.GUI.IsoCat(map, (fCraft.GUI.IsoCatMode)mode, rotation);
                renderer.ChunkCoords = chunkCoords;

                Rectangle cropRectangle;
                BackgroundWorker bwRenderer = new BackgroundWorker();
                bwRenderer.WorkerReportsProgress = true;
                Bitmap rawImage = renderer.Draw( out cropRectangle, bwRenderer );

                Bitmap outputImage = rawImage.Clone(cropRectangle, rawImage.PixelFormat);
                outputImage.Save(output, System.Drawing.Imaging.ImageFormat.Png);
            } catch (Exception ex) {
                Console.WriteLine("An Error Occured!");
                Console.Write(ex);
            }
        }
    }
}