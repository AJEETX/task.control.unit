using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using risk.control.system.Models.ViewModel;
using risk.control.system.Services;

namespace risk.control.system.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class IcheckifyApiController : ControllerBase
    {
        private readonly IHttpClientService httpClientService;
        private static string BaseUrl = "http://icheck-webSe-kOnc2X2NMOwe-196777346.ap-southeast-2.elb.amazonaws.com";
        private static string PanIdfyUrl = "https://idfy-verification-suite.p.rapidapi.com";
        private static string RapidAPIKey = "df0893831fmsh54225589d7b9ad1p15ac51jsnb4f768feed6f";
        private static string PanTask_id = "74f4c926-250c-43ca-9c53-453e87ceacd1";
        private static string PanGroup_id = "8e16424a-58fc-4ba4-ab20-5bc8e7c3c41e";

        public IcheckifyApiController(IHttpClientService httpClientService)
        {
            this.httpClientService = httpClientService;
        }

        [HttpPost("mask")]
        public async Task<IActionResult> Mask(MaskImage image)
        {
            var maskedImageDetail = await httpClientService.GetMaskedImage(image, BaseUrl);

            return Ok(maskedImageDetail);
        }

        [HttpPost("match")]
        public async Task<IActionResult> Match(MatchImage image)
        {
            var maskedImageDetail = await httpClientService.GetFaceMatch(image, BaseUrl);

            return Ok(maskedImageDetail);
        }

        [HttpGet("pan")]
        public async Task<IActionResult> Pan(string pan = "FNLPM8635N")
        {
            var verifiedPanResponse = await httpClientService.VerifyPan(pan, PanIdfyUrl, RapidAPIKey, PanTask_id, PanGroup_id);

            return Ok(verifiedPanResponse);
        }

        [HttpGet("GetAddressByLatLng")]
        public async Task<IActionResult> GetAddressByLatLng(string lat, string lng)
        {
            var verifiedPanResponse = await httpClientService.GetAddress(lat, lng);

            return Ok(verifiedPanResponse);
        }
    }
}