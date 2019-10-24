using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace PhotoSort
{
    class PhotoSort
    {
        //public async Task<string> GetFolderPathAsync()
        //{
        //    CommonOpenFileDialog dialog = new CommonOpenFileDialog();
        //    string path = string.Empty;

        //    dialog.IsFolderPicker = true;

        //    await Task.Run(() =>
        //    {
        //        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        //        {
        //            path = dialog.FileName;
        //        }
        //    });

        //    return path;
        //}

        // to do for each file:
        // get list of files and work off the the list so files can be moved around
        // get the following data: date taken, file path, file size
        // make sure they're date taken ascending order
        // create the first disc folder "Disc 01"
        // create a folder based on date taken "(month) (year)"
        // move file into month folder
        // create and fill new month folders until the disc folder reaches a certain size
        // when the first disc is full, create a new disc folder
        // create the month folder
        // move file into new month folder

        public bool ParseFiles(string sourceFolder, string destinationFolder, long bytesPerDisc)
        {
            List<PhotoItem> photoItems = GetListOfFiles(sourceFolder);

            if (photoItems.Count == 0)
            {
                throw new Exception("No photos found to sort.");
            }

            // sort by date taken ascending (should already be in that order, but make sure)
            photoItems.Sort((x, y) => x.dateTaken.CompareTo(y.dateTaken));

            long currentBytes = 0;
            int currentDisc = 1;

            // create first folder, since that's always going to happen

            foreach (PhotoItem item in photoItems)
            {
                currentBytes += item.fileSize;
                
                if (currentBytes > bytesPerDisc)
                {
                    currentDisc++; // increment the disc to a new folder
                    currentBytes = item.fileSize; // reset the current bytes to the current file
                }

                DirectoryInfo discFolder = Directory.CreateDirectory(destinationFolder + String.Format("/Disc {0:D2}", currentDisc)); // will create only if it's missing
                DirectoryInfo monthFolder = Directory.CreateDirectory(discFolder.FullName + "/" + item.dateTaken.ToString("yyyy-MM (MMM)"));//item.dateTaken.ToString("MMMM yyyy"));

                FileInfo file = new FileInfo(item.filePath);
                file.MoveTo(monthFolder.FullName + "/" + file.Name);
            }

            // check each folder and remove the empty ones (if working in the same folder)
            if (sourceFolder.ToLower() == destinationFolder.ToLower())
            {
                foreach (string folderPath in Directory.GetDirectories(destinationFolder, "*", SearchOption.AllDirectories))
                {
                    if (Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories).Length == 0)
                    {
                        // delete the folder only if no files come back
                        Directory.Delete(folderPath, true);
                    }
                }
            }

            return true;
        }

        private List<PhotoItem> GetListOfFiles(string sourceFolder)
        {
            Regex r = new Regex(":"); // for sanitizing EXIF date tag
            List<PhotoItem> photoItems = new List<PhotoItem>();

            foreach (string filePath in Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories))
            {
                // from https://stackoverflow.com/a/7713780
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (Image image = Image.FromStream(fileStream, false, false))
                    {
                        if (image.PropertyIdList.Any(x => x == 36867)) // 36867 is ID of DateTimeOriginal/DateTimeTaken
                        {
                            PropertyItem dateTimeTaken = image.GetPropertyItem(36867);
                            string dateTakenString = r.Replace(Encoding.UTF8.GetString(dateTimeTaken.Value), "-", 2);
                            DateTime dateTaken = DateTime.Parse(dateTakenString);

                            photoItems.Add(new PhotoItem()
                            {
                                dateTaken = dateTaken,
                                filePath = fileStream.Name,
                                fileSize = fileStream.Length
                            });
                        }
                        else
                        {
                            throw new InvalidDataException(String.Format("File has no \"Date Taken\" tag: {0}", fileStream.Name));
                        }
                    }
                }

                //IEnumerable<MetadataExtractor.Directory> things = MetadataExtractor.ImageMetadataReader.ReadMetadata(filePath);

                // want EXIF DateTime Original
            }

            return photoItems;
        }
    }

    class PhotoItem
    {
        public string filePath;
        public DateTime dateTaken;
        public long fileSize;
    }
}
