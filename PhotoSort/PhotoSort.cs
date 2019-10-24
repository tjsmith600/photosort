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
        List<PhotoItem> photoItems;

        public async Task<string> GetFolderPathAsync()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            string path = string.Empty;

            dialog.IsFolderPicker = true;

            await Task.Run(() =>
            {
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    path = dialog.FileName;
                }
            });

            return path;
        }

        public bool ParseFiles(string sourceFolder)
        {
            photoItems = new List<PhotoItem>(); // clear old cache, if necesary

            Regex r = new Regex(":"); // for sanitizing EXIF date tag

            foreach(string filePath in Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories))
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

                            photoItems.Add(new PhotoItem() { 
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

            return true;
        }
    }

    class PhotoItem
    {
        public string filePath;
        public DateTime dateTaken;
        public long fileSize;
    }
}
