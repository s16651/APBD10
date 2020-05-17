using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APBD10.DTOs.Requests;
using APBD10.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD10.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IDbService _db;
        public StudentsController(IDbService db)
        {
            _db = db;
        }
        [HttpGet]
        public IActionResult GetStudents()
        {
            var students = _db.GetAllStudents();
            return Ok(students);
        }
        [HttpPost]
        public async Task<IActionResult> InsertStudentAsync(InsertStudentRequest isq)
        {
            int i = 0;
            i = await _db.AddStudentAsync(isq);
            if (i == -1)
            {
                return BadRequest();
            }
            return Ok("Student inserted");
        }
        [HttpPut]
        public async Task<IActionResult> UpdateStudentAsync(ModifyStudentRequest msr)
        {
            int i = 0;
            i = await _db.ModifyStudentAsync(msr);
            if (i == -1)
            {
                return NotFound("Student does not exist");
            }
            return Ok("Student updated");
        }
        [HttpDelete("{index}")]
        public async Task<IActionResult> DeleteStudentAsync(string index)
        {
            int i = 0;
            i = await _db.RemoveStudentAsync(index);
            if (i == -1)
            {
                return NotFound("Student does not exist");
            }
            return Ok("Student deleted");
        }

    }
}