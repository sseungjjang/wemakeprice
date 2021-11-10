using Microsoft.AspNetCore.Mvc;
using mvcAssignment.Models;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml;

namespace mvcAssignment.Controllers
{
    class Result
    {
        public string shareVal { get; set; }
        public string restVal { get; set; }
        public string error { get; set; }
    }
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        //출력 ajax 호출 처리
        [HttpPost]
        public JsonResult ExportStr(string url, string type, int bundle)
        {
            Result _result;
            try
            {
                //입력받은 URL값으로 호출
                WebRequest request = WebRequest.Create(url);
                request.Credentials = CredentialCache.DefaultCredentials;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("잘못된 URL입력");
                }

                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string resString = reader.ReadToEnd();

                string concatStr = string.Empty;

                string numStr = string.Empty;
                string alphaStr = string.Empty;

                //HTML 태그 제외 타입일때 태그를 정규식으로 삭제하고 나머지 text값만 추출
                //type - A : HTML 태그 제외, B : Text 전체
                if (type == "A")
                {
                    resString = Regex.Replace(resString, @"<(./\n)*?>", string.Empty);
                }

                //숫자만 정규식으로 추출
                numStr = Regex.Replace(resString, @"[^0-9]", "");
                //영문만 정규시으로 추출
                alphaStr = Regex.Replace(resString, @"[^a-zA-Z]", "");

                //추출한 숫자를 오름차순으로 정렬
                char[] numArr = numStr.ToCharArray();
                Array.Sort(numArr);

                //추출한 영문을 대/소문자 상관없이 오름차순 정렬
                string sortedStr = string.Concat(alphaStr.OrderBy(char.ToLower).ThenBy(char.IsLower));
                char[] alphaArr = sortedStr.ToCharArray();


                //교차출력을 위해 추출한 숫자와 영문을 교차로 배열에 저장
                int sumCnt = (numArr.Length + alphaArr.Length);

                char[] crossStr = new char[sumCnt];

                int alphaCnt = 0;
                int numCnt = 0;

                for (int i = 0; i < sumCnt; i++)
                {
                    //영문을 저장하고 영문이 없을때 숫자를 저장
                    if (i % 2 == 0)
                    {
                        if (alphaCnt < alphaArr.Length)
                        {
                            crossStr[i] = alphaArr[alphaCnt];
                            alphaCnt++;
                        }
                        else if (numCnt < numArr.Length)
                        {
                            crossStr[i] = numArr[numCnt];
                            numCnt++;
                        }
                    }
                    else  //숫자를 저장하고 숫자가 없을때 영문을 저장
                    {
                        if (numCnt < numArr.Length)
                        {
                            crossStr[i] = numArr[numCnt];
                            numCnt++;
                        }
                        else if (alphaCnt < alphaArr.Length)
                        {
                            crossStr[i] = alphaArr[alphaCnt];
                            alphaCnt++;
                        }
                    }
                }

                //교차로 저장한 배열을 합여서 문자열로 변환
                concatStr = string.Concat(crossStr);

                //출력값 리턴
                _result = new Result
                {
                    //출력단위묶음과 몫의 곱한값의 교차문자열 길이 값
                    shareVal = concatStr.Substring(0, bundle * (concatStr.Length / bundle)),
                    //출력단위묶음과 몫의 곱한값을 제외한 나머지 값
                    restVal = concatStr.Substring(concatStr.Length - (concatStr.Length % bundle))
                };
            }
            catch(Exception Ex)
            {
                _result = new Result
                {
                    error = Ex.Message
                };
            }
            return Json(_result);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}