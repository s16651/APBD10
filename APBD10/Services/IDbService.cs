using APBD10.DTOs.Requests;
using APBD10.DTOs.Responses;
using APBD10.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APBD10.Services
{
   public interface IDbService
    {
        public IEnumerable<GetStudentsResponse> GetAllStudents();
        public Task<int> AddStudentAsync(InsertStudentRequest isq);
        public Task<int> ModifyStudentAsync(ModifyStudentRequest msr);
        public Task<int> RemoveStudentAsync(string index);
        public Task<EnrollmentStudentResponse> EnrollStudentAsync(EnrollmentStudenRequest request);
        public Task<PromoteStudentResponse> PromoteStudentsAsync(PromoteStudentRequest promoteStudentRequest);
    }
}
