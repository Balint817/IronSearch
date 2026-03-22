using System.IO.Compression;
using CustomAlbums.Data;
using NAudio.Vorbis;
using NLayer;

namespace IronSearch
{

    public static class AudioHelper
    {
        public static TimeSpan? GetAlbumLength(object albumBoxed)
        {

            if (albumBoxed is not Album album)
            {
                throw new ArgumentException("invalid argument type", nameof(albumBoxed));
            }

            if (album == null || string.IsNullOrEmpty(album.Path))
                return null;

            try
            {
                if (album.IsPackaged)
                {
                    return GetFromZip(album.Path);
                }
                else
                {
                    return GetFromDirectory(album.Path);
                }
            }
            catch
            {
                return null;
            }
        }

        private static TimeSpan? GetFromDirectory(string dirPath)
        {
            string oggPath = Path.Combine(dirPath, "music.ogg");
            if (File.Exists(oggPath))
            {
                using var fs = File.OpenRead(oggPath);
                return GetOggLength(fs);
            }

            string mp3Path = Path.Combine(dirPath, "music.mp3");
            if (File.Exists(mp3Path))
            {
                using var fs = File.OpenRead(mp3Path);
                return GetMp3Length(fs);
            }

            return null;
        }

        private static TimeSpan? GetFromZip(string zipPath)
        {
            using var archive = ZipFile.OpenRead(zipPath);

            // Prefer OGG
            var oggEntry = archive.GetEntry("music.ogg");
            if (oggEntry != null)
            {
                using var stream = oggEntry.Open();
                return GetOggLength(stream);
            }

            var mp3Entry = archive.GetEntry("music.mp3");
            if (mp3Entry != null)
            {
                using var stream = mp3Entry.Open();
                return GetMp3Length(stream);
            }

            return null;
        }

        private static TimeSpan? GetOggLength(Stream stream)
        {
            try
            {
                using var reader = new VorbisWaveReader(stream);
                return reader.TotalTime;
            }
            catch
            {
                return null;
            }
        }

        private static TimeSpan? GetMp3Length(Stream stream)
        {
            try
            {
                using var mpeg = new MpegFile(stream);
                return mpeg.Length;
            }
            catch
            {
                return null;
            }
        }
    }
}
