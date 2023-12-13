//using Microsoft.AspNetCore.Mvc;
//using System.Data;

//namespace risk.control.system.Controllers
//{
//    public class UploadPinCodeController : Controller
//    {
//        private readonly IWebHostEnvironment webHostEnvironment;

//        public UploadPinCodeController(IWebHostEnvironment webHostEnvironment)
//        {
//            this.webHostEnvironment = webHostEnvironment;
//        }
//        [HttpPost]
//        public async Task<IActionResult> Upload(IFormFile postedFile)
//        {
//            if (postedFile != null)
//            {
//                string path = Path.Combine(webHostEnvironment.WebRootPath, "upload-pincodes");
//                if (!Directory.Exists(path))
//                {
//                    Directory.CreateDirectory(path);
//                }

//                string fileName = Path.GetFileName(postedFile.FileName);
//                string filePath = Path.Combine(path, fileName);
//                using (FileStream stream = new FileStream(filePath, FileMode.Create))
//                {
//                    postedFile.CopyTo(stream);
//                }
//                string csvData = await System.IO.File.ReadAllTextAsync(filePath);
//                DataTable dt = new DataTable();
//                bool firstRow = true;
//                foreach (string row in csvData.Split('\n'))
//                {
//                    if (!string.IsNullOrEmpty(row))
//                    {
//                        if (!string.IsNullOrEmpty(row))
//                        {
//                            if (firstRow)
//                            {
//                                foreach (string cell in row.Split(','))
//                                {
//                                    dt.Columns.Add(cell.Trim());
//                                }
//                                firstRow = false;
//                            }
//                            else
//                            {
//                                dt.Rows.Add();
//                                int i = 0;
//                                foreach (string cell in row.Split(','))
//                                {
//                                    dt.Rows[dt.Rows.Count - 1][i] = cell.Trim();
//                                    i++;
//                                }
//                            }
//                        }
//                    }
//                }

//            }
//            return View();
//        }
//    }
//}
