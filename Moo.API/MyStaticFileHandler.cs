
namespace Moo.API
{
    using System;
    using System.Globalization;
    using System.Web;

    static class HttpDate
    {
        private static readonly string[] s_days = new string[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
        private static readonly sbyte[] s_monthIndexTable = new sbyte[] { 
            -1, 0x41, 2, 12, -1, -1, -1, 8, -1, -1, -1, -1, 7, -1, 0x4e, -1, 
            9, -1, 0x52, -1, 10, -1, 11, -1, -1, 5, -1, -1, -1, -1, -1, -1, 
            -1, 0x41, 2, 12, -1, -1, -1, 8, -1, -1, -1, -1, 7, -1, 0x4e, -1, 
            9, -1, 0x52, -1, 10, -1, 11, -1, -1, 5, -1, -1, -1, -1, -1, -1
         };
        private static readonly string[] s_months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        private static readonly int[] s_tensDigit = new int[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90 };

        private static int atoi2(string s, int startIndex)
        {
            int num3;
            try
            {
                int index = s[startIndex] - '0';
                int num2 = s[1 + startIndex] - '0';
                num3 = s_tensDigit[index] + num2;
            }
            catch
            {
                throw new FormatException("Atio2BadString");
            }
            return num3;
        }

        private static int make_month(string s, int startIndex)
        {
            int index = (s[2 + startIndex] - '@') & 0x3f;
            sbyte num2 = s_monthIndexTable[index];
            if (num2 >= 13)
            {
                if (num2 != 0x4e)
                {
                    if (num2 != 0x52)
                    {
                        throw new FormatException("MakeMonthBadString");
                    }
                    if (s_monthIndexTable[(s[1 + startIndex] - '@') & 0x3f] == 0x41)
                    {
                        num2 = 3;
                    }
                    else
                    {
                        num2 = 4;
                    }
                }
                else if (s_monthIndexTable[(s[1 + startIndex] - '@') & 0x3f] == 0x41)
                {
                    num2 = 1;
                }
                else
                {
                    num2 = 6;
                }
            }
            string str = s_months[num2 - 1];
            if ((((s[startIndex] != str[0]) || (s[1 + startIndex] != str[1])) || (s[2 + startIndex] != str[2])) && (((char.ToUpper(s[startIndex], CultureInfo.InvariantCulture) != str[0]) || (char.ToLower(s[1 + startIndex], CultureInfo.InvariantCulture) != str[1])) || (char.ToLower(s[2 + startIndex], CultureInfo.InvariantCulture) != str[2])))
            {
                throw new FormatException("MakeMonthBadString");
            }
            return num2;
        }

        internal static DateTime UtcParse(string time)
        {
            int num2;
            int num3;
            int num4;
            int num5;
            int num6;
            int num7;
            if (time == null)
            {
                throw new ArgumentNullException("time");
            }
            int index = time.IndexOf(',');
            if (index != -1)
            {
                int num8 = time.Length - index;
                while ((--num8 > 0) && (time[++index] == ' '))
                {
                }
                if (time[index + 2] == '-')
                {
                    if (num8 < 0x12)
                    {
                        throw new FormatException("UtilParseDateTimeBad");
                    }
                    num4 = atoi2(time, index);
                    num3 = make_month(time, index + 3);
                    num2 = atoi2(time, index + 7);
                    if (num2 < 50)
                    {
                        num2 += 0x7d0;
                    }
                    else
                    {
                        num2 += 0x76c;
                    }
                    num5 = atoi2(time, index + 10);
                    num6 = atoi2(time, index + 13);
                    num7 = atoi2(time, index + 0x10);
                }
                else
                {
                    if (num8 < 20)
                    {
                        throw new FormatException("UtilParseDateTimeBad");
                    }
                    num4 = atoi2(time, index);
                    num3 = make_month(time, index + 3);
                    num2 = (atoi2(time, index + 7) * 100) + atoi2(time, index + 9);
                    num5 = atoi2(time, index + 12);
                    num6 = atoi2(time, index + 15);
                    num7 = atoi2(time, index + 0x12);
                }
            }
            else
            {
                index = -1;
                int num9 = time.Length + 1;
                while ((--num9 > 0) && (time[++index] == ' '))
                {
                }
                if (num9 < 0x18)
                {
                    throw new FormatException("UtilParseDateTimeBad");
                }
                num4 = atoi2(time, index + 8);
                num3 = make_month(time, index + 4);
                num2 = (atoi2(time, index + 20) * 100) + atoi2(time, index + 0x16);
                num5 = atoi2(time, index + 11);
                num6 = atoi2(time, index + 14);
                num7 = atoi2(time, index + 0x11);
            }
            return new DateTime(num2, num3, num4, num5, num6, num7, DateTimeKind.Utc);
        }
    }
}

namespace Moo.API
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Web.Hosting;
    using System.Web.Util;
    using System.Reflection;
    using System.Web;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ByteRange
    {
        internal long Offset;
        internal long Length;
    }

    public class MyStaticFileHandler : IHttpHandler
    {
        private const string CONTENT_RANGE_FORMAT = "bytes {0}-{1}/{2}";
        private const int ERROR_ACCESS_DENIED = 5;
        private const int MAX_RANGE_ALLOWED = 5;
        private const string MULTIPART_CONTENT_TYPE = "multipart/byteranges; boundary=<q1w2e3r4t5y6u7i8o9p0zaxscdvfbgnhmjklkl>";
        private const string MULTIPART_RANGE_DELIMITER = "--<q1w2e3r4t5y6u7i8o9p0zaxscdvfbgnhmjklkl>\r\n";
        private const string MULTIPART_RANGE_END = "--<q1w2e3r4t5y6u7i8o9p0zaxscdvfbgnhmjklkl>--\r\n\r\n";
        private const string RANGE_BOUNDARY = "<q1w2e3r4t5y6u7i8o9p0zaxscdvfbgnhmjklkl>";

        private static string GenerateETag(HttpContext context, DateTime lastModified, DateTime now)
        {
            long num = lastModified.ToFileTime();
            long num2 = now.ToFileTime();
            string str = num.ToString("X8", CultureInfo.InvariantCulture);
            if ((num2 - num) <= 0x1c9c380L)
            {
                return ("W/\"" + str + "\"");
            }
            return ("\"" + str + "\"");
        }

        private static FileInfo GetFileInfo(string virtualPathWithPathInfo, string physicalPath, HttpResponse response)
        {
            FileInfo info;
            if (!new FileInfo(physicalPath).Exists)
            {
                throw new HttpException(0x194, "File_does_not_exist");
            }
            if (physicalPath[physicalPath.Length - 1] == '.')
            {
                throw new HttpException(0x194, "File_does_not_exist");
            }
            try
            {
                info = new FileInfo(physicalPath);
            }
            catch (IOException exception)
            {
                throw new HttpException(0x194, "Error_trying_to_enumerate_files", exception);
            }
            catch (SecurityException exception2)
            {
                throw new HttpException(0x191, "File_enumerator_access_denied", exception2);
            }
            if ((info.Attributes & FileAttributes.Hidden) != 0)
            {
                throw new HttpException(0x194, "File_is_hidden");
            }
            if ((info.Attributes & FileAttributes.Directory) != 0)
            {
                if (virtualPathWithPathInfo.EndsWith("/"))
                {
                    throw new HttpException(0x193, "Missing_star_mapping");
                }
                response.Redirect(virtualPathWithPathInfo + "/");
            }
            return info;
        }

        private static bool GetLongFromSubstring(string s, ref int startIndex, out long result)
        {
            result = 0L;
            MovePastSpaceCharacters(s, ref startIndex);
            int num = startIndex;
            MovePastDigits(s, ref startIndex);
            int num2 = startIndex - 1;
            if (num2 < num)
            {
                return false;
            }
            long num3 = 1L;
            for (int i = num2; i >= num; i--)
            {
                int num5 = s[i] - '0';
                result += num5 * num3;
                num3 *= 10L;
                if (result < 0L)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool GetNextRange(string rangeHeader, ref int startIndex, long fileLength, out long offset, out long length, out bool isSatisfiable)
        {
            long num;
            offset = 0L;
            length = 0L;
            isSatisfiable = false;
            if (fileLength <= 0L)
            {
                startIndex = rangeHeader.Length;
                return true;
            }
            MovePastSpaceCharacters(rangeHeader, ref startIndex);
            if ((startIndex < rangeHeader.Length) && (rangeHeader[startIndex] == '-'))
            {
                startIndex++;
                if (!GetLongFromSubstring(rangeHeader, ref startIndex, out length))
                {
                    return false;
                }
                if (length > fileLength)
                {
                    offset = 0L;
                    length = fileLength;
                }
                else
                {
                    offset = fileLength - length;
                }
                isSatisfiable = IsRangeSatisfiable(offset, length, fileLength);
                return IncrementToNextRange(rangeHeader, ref startIndex);
            }
            if (GetLongFromSubstring(rangeHeader, ref startIndex, out offset) && ((startIndex < rangeHeader.Length) && (rangeHeader[startIndex] == '-')))
            {
                startIndex++;
            }
            else
            {
                return false;
            }
            if (!GetLongFromSubstring(rangeHeader, ref startIndex, out num))
            {
                length = fileLength - offset;
            }
            else
            {
                if (num > (fileLength - 1L))
                {
                    num = fileLength - 1L;
                }
                length = (num - offset) + 1L;
                if (length < 1L)
                {
                    return false;
                }
            }
            isSatisfiable = IsRangeSatisfiable(offset, length, fileLength);
            return IncrementToNextRange(rangeHeader, ref startIndex);
        }

        private static bool IncrementToNextRange(string s, ref int startIndex)
        {
            MovePastSpaceCharacters(s, ref startIndex);
            if (startIndex < s.Length)
            {
                if (s[startIndex] != ',')
                {
                    return false;
                }
                startIndex++;
            }
            return true;
        }

        private static bool IsOutDated(string ifRangeHeader, DateTime lastModified)
        {
            try
            {
                DateTime time = lastModified.ToUniversalTime();
                return (Moo.API.HttpDate.UtcParse(ifRangeHeader) < time);
            }
            catch
            {
                return true;
            }
        }

        private static bool IsRangeSatisfiable(long offset, long length, long fileLength)
        {
            return ((offset < fileLength) && (length > 0L));
        }

        private static bool IsSecurityError(int ErrorCode)
        {
            return (ErrorCode == 5);
        }

        private static void MovePastDigits(string s, ref int startIndex)
        {
            while (((startIndex < s.Length) && (s[startIndex] <= '9')) && (s[startIndex] >= '0'))
            {
                startIndex++;
            }
        }

        private static void MovePastSpaceCharacters(string s, ref int startIndex)
        {
            while ((startIndex < s.Length) && (s[startIndex] == ' '))
            {
                startIndex++;
            }
        }

        internal static unsafe void memcpyimpl(byte* src, byte* dest, int len)
        {
            if (len >= 0x10)
            {
                do
                {
                    *((int*)dest) = *((int*)src);
                    *((int*)(dest + 4)) = *((int*)(src + 4));
                    *((int*)(dest + 8)) = *((int*)(src + 8));
                    *((int*)(dest + 12)) = *((int*)(src + 12));
                    dest += 0x10;
                    src += 0x10;
                }
                while ((len -= 0x10) >= 0x10);
            }
            if (len > 0)
            {
                if ((len & 8) != 0)
                {
                    *((int*)dest) = *((int*)src);
                    *((int*)(dest + 4)) = *((int*)(src + 4));
                    dest += 8;
                    src += 8;
                }
                if ((len & 4) != 0)
                {
                    *((int*)dest) = *((int*)src);
                    dest += 4;
                    src += 4;
                }
                if ((len & 2) != 0)
                {
                    *((short*)dest) = *((short*)src);
                    dest += 2;
                    src += 2;
                }
                if ((len & 1) != 0)
                {
                    dest++;
                    src++;
                    dest[0] = src[0];
                }
            }
        }

        internal static unsafe bool ProcessRangeRequest(HttpContext context, string physicalPath, long fileLength, string rangeHeader, string etag, DateTime lastModified)
        {

            long offset;
            long length;
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            bool flag = false;
            if (fileLength <= 0L)
            {
                SendRangeNotSatisfiable(response, fileLength);
                return true;
            }
            string ifRangeHeader = request.Headers["If-Range"];
            if ((ifRangeHeader != null) && (ifRangeHeader.Length > 1))
            {
                if (ifRangeHeader[0] == '"')
                {
                    if (ifRangeHeader != etag)
                    {
                        return flag;
                    }
                }
                else
                {
                    if ((ifRangeHeader[0] == 'W') && (ifRangeHeader[1] == '/'))
                    {
                        return flag;
                    }
                    if (IsOutDated(ifRangeHeader, lastModified))
                    {
                        return flag;
                    }
                }
            }
            int index = rangeHeader.IndexOf('=');
            if ((index == -1) || (index == (rangeHeader.Length - 1)))
            {
                return flag;
            }
            int startIndex = index + 1;
            bool flag2 = true;
            bool flag4 = false;
            ByteRange[] rangeArray = null;
            int num5 = 0;
            long num6 = 0L;
            while ((startIndex < rangeHeader.Length) && flag2)
            {
                bool flag3;
                flag2 = GetNextRange(rangeHeader, ref startIndex, fileLength, out offset, out length, out flag3);
                if (!flag2)
                {
                    break;
                }
                if (flag3)
                {
                    if (rangeArray == null)
                    {
                        rangeArray = new ByteRange[0x10];
                    }
                    if (num5 >= rangeArray.Length)
                    {
                        ByteRange[] rangeArray2 = new ByteRange[rangeArray.Length * 2];
                        int len = rangeArray.Length * Marshal.SizeOf(rangeArray[0]);
                        fixed (ByteRange* rangeRef = rangeArray)
                        {
                            fixed (ByteRange* rangeRef2 = rangeArray2)
                            {
                                memcpyimpl((byte*)rangeRef, (byte*)rangeRef2, len);
                            }
                        }
                        rangeArray = rangeArray2;
                    }
                    rangeArray[num5].Offset = offset;
                    rangeArray[num5].Length = length;
                    num5++;
                    num6 += length;
                    if (num6 > (fileLength * 5L))
                    {
                        flag4 = true;
                        break;
                    }
                }
            }
            if (!flag2)
            {
                return flag;
            }
            if (flag4)
            {
                SendBadRequest(response);
                return true;
            }
            if (num5 == 0)
            {
                SendRangeNotSatisfiable(response, fileLength);
                return true;
            }
            string mimeMapping = MimeMapping.GetMimeMapping(physicalPath);
            if (num5 == 1)
            {
                offset = rangeArray[0].Offset;
                length = rangeArray[0].Length;
                response.ContentType = mimeMapping;
                string str3 = string.Format(CultureInfo.InvariantCulture, "bytes {0}-{1}/{2}", new object[] { offset, (offset + length) - 1L, fileLength });
                response.AppendHeader("Content-Range", str3);
                SendFile(physicalPath, offset, length, fileLength, context);
            }
            else
            {
                response.ContentType = "multipart/byteranges; boundary=<q1w2e3r4t5y6u7i8o9p0zaxscdvfbgnhmjklkl>";
                string s = "Content-Type: " + mimeMapping + "\r\n";
                for (int i = 0; i < num5; i++)
                {
                    offset = rangeArray[i].Offset;
                    length = rangeArray[i].Length;
                    response.Write("--<q1w2e3r4t5y6u7i8o9p0zaxscdvfbgnhmjklkl>\r\n");
                    response.Write(s);
                    response.Write("Content-Range: ");
                    string str4 = string.Format(CultureInfo.InvariantCulture, "bytes {0}-{1}/{2}", new object[] { offset, (offset + length) - 1L, fileLength });
                    response.Write(str4);
                    response.Write("\r\n\r\n");
                    SendFile(physicalPath, offset, length, fileLength, context);
                    response.Write("\r\n");
                }
                response.Write("--<q1w2e3r4t5y6u7i8o9p0zaxscdvfbgnhmjklkl>--\r\n\r\n");
            }
            response.StatusCode = 0xce;
            response.AppendHeader("Last-Modified", (string)typeof(HttpUtility).GetMethod("FormatHttpDateTime").Invoke(typeof(HttpUtility), new object[] { lastModified }));
            response.AppendHeader("Accept-Ranges", "bytes");
            response.AppendHeader("ETag", etag);
            response.AppendHeader("Cache-Control", "public");
            return true;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void ProcessRequest(HttpContext context)
        {
            ProcessRequestInternal(context, null);
        }

        private static bool ProcessRequestForNonMapPathBasedVirtualFile(HttpRequest request, HttpResponse response, string overrideVirtualPath)
        {
            /*
            bool flag = false;

            var isUsing = (bool)typeof(HostingEnvironment).GetProperty("UsingMapPathBasedVirtualPathProvider",BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.).GetValue(typeof(HostingEnvironment));

            if (isUsing)
            {
                return flag;
            }
            VirtualFile vf = null;
            string virtualPath = (overrideVirtualPath == null) ? request.FilePath : overrideVirtualPath;
            if (HostingEnvironment.VirtualPathProvider.FileExists(virtualPath))
            {
                vf = HostingEnvironment.VirtualPathProvider.GetFile(virtualPath);
            }
            if (vf == null)
            {
                throw new HttpException(0x194, "File_does_not_exist");
            }


            if (typeof(System.Web.IHtmlString).Assembly.GetType("System.Web.MapPathBasedVirtualFile").IsInstanceOfType(vf))
            {
                return flag;
            }

            typeof(HttpResponse).GetMethod("WriteVirtualFile").Invoke(response, new object[] { vf });
            response.ContentType = MimeMapping.GetMimeMapping(virtualPath);
            return true;
             * */
            return false;
        }

        internal static void ProcessRequestInternal(HttpContext context, string overrideVirtualPath)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            if (!ProcessRequestForNonMapPathBasedVirtualFile(request, response, overrideVirtualPath))
            {
                string path;
                string physicalPath;
                if (overrideVirtualPath == null)
                {
                    path = request.Path;
                    physicalPath = request.PhysicalPath;
                }
                else
                {
                    path = overrideVirtualPath;
                    physicalPath = request.MapPath(overrideVirtualPath);
                }
                FileInfo info = GetFileInfo(path, physicalPath, response);
                DateTime lastModified = new DateTime(info.LastWriteTimeUtc.Year, info.LastWriteTimeUtc.Month, info.LastWriteTimeUtc.Day, info.LastWriteTimeUtc.Hour, info.LastWriteTimeUtc.Minute, info.LastWriteTimeUtc.Second, 0, DateTimeKind.Utc);
                DateTime utcNow = DateTime.UtcNow;
                if (lastModified > utcNow)
                {
                    lastModified = new DateTime(utcNow.Ticks - (utcNow.Ticks % 0x989680L), DateTimeKind.Utc);
                }
                string etag = GenerateETag(context, lastModified, utcNow);
                long length = info.Length;
                string str4 = request.Headers["Range"];
                if (str4 == null || !str4.StartsWith("bytes") || !ProcessRangeRequest(context, physicalPath, length, str4, etag, lastModified))
                {
                    SendFile(physicalPath, 0L, length, length, context);
                    response.ContentType = MimeMapping.GetMimeMapping(physicalPath);
                    response.AppendHeader("Accept-Ranges", "bytes");
                    if (request.QueryString["filename"] != null)
                        response.AppendHeader("Content-Disposition", "attachment; filename=" + request.QueryString["filename"]);
                    response.AddFileDependency(physicalPath);
                    typeof(HttpCachePolicy).GetMethod("SetIgnoreRangeRequests", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(response.Cache, new object[0]);
                    //response.Cache.SetIgnoreRangeRequests();
                    response.Cache.SetExpires(utcNow.AddDays(1.0));
                    response.Cache.SetLastModified(lastModified);
                    response.Cache.SetETag(etag);
                    response.Cache.SetCacheability(HttpCacheability.Public);
                }
            }
        }

        private static void SendBadRequest(HttpResponse response)
        {
            response.StatusCode = 400;
            response.Write("<html><body>Bad Request</body></html>");
        }

        private static void SendFile(string physicalPath, long offset, long length, long fileLength, HttpContext context)
        {
            try
            {
                context.Response.TransmitFile(physicalPath, offset, length);
            }
            catch (ExternalException exception)
            {
                if (IsSecurityError(exception.ErrorCode))
                {
                    throw new HttpException(0x191, "Resource_access_forbidden");
                }
                throw;
            }
        }

        private static void SendRangeNotSatisfiable(HttpResponse response, long fileLength)
        {
            response.StatusCode = 0x1a0;
            response.ContentType = null;
            response.AppendHeader("Content-Range", "bytes */" + fileLength.ToString(NumberFormatInfo.InvariantInfo));
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}
