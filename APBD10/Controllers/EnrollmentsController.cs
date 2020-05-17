using APBD10.DTOs.Requests;
using APBD10.DTOs.Responses;
using APBD10.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace APBD10.Controllers
{
        [Route("api/enrollment")]
        [ApiController]
        public class EnrollmentsController : ControllerBase
        {
            private readonly IDbService _db;
            public EnrollmentsController(IDbService db)
            {
                _db = db;
            }
            [HttpPost]
            public async Task<IActionResult> EnrollStudentAsync(EnrollmentStudenRequest request)
            {
                EnrollmentStudentResponse enrollStudentResponse = await _db.EnrollStudentAsync(request);
                if (enrollStudentResponse == null)
                {
                    return BadRequest();
                }
                return this.StatusCode(201, enrollStudentResponse);
            }
            [HttpPost("promote")]
            public async Task<IActionResult> PromoteStudents(PromoteStudentRequest promoteStudentRequest)
            {
                PromoteStudentResponse promoteStudentResponse = await _db.PromoteStudentsAsync(promoteStudentRequest);
                if (promoteStudentResponse == null)
                {
                    return NotFound();
                }
                return this.StatusCode(201, promoteStudentResponse);
            }
        } 
}