using APBD10.DTOs.Requests;
using APBD10.DTOs.Responses;
using APBD10.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APBD10.Services
{
    public class SqlDbService : IDbService
    {
        private readonly StudentContext _studentContext;
        public SqlDbService(StudentContext studentContext)
        {
            _studentContext = studentContext;
        }
        public IEnumerable<GetStudentsResponse> GetAllStudents()
        {
            var students = _studentContext.Student.Include(s => s.IdEnrollmentNavigation)
                                                    .ThenInclude(s => s.IdStudyNavigation)
                                                    .Select(st => new GetStudentsResponse
                                                    {
                                                        IndexNumber = st.IndexNumber,
                                                        FirstName = st.FirstName,
                                                        LastName = st.LastName,
                                                        BirthDate = st.BirthDate,
                                                        Semester = st.IdEnrollmentNavigation.Semester,
                                                        Studies = st.IdEnrollmentNavigation.IdStudyNavigation.Name,
                                                        Password = st.Password
                                                    }).ToList();
            return students;
        }
        public async Task<int> AddStudentAsync(InsertStudentRequest isq)
        {
            try
            {
                var st = new Student
                {
                    IndexNumber = isq.IndexNumber,
                    FirstName = isq.FirstName,
                    LastName = isq.LastName,
                    BirthDate = isq.BirthDate,
                    IdEnrollment = isq.IdEnrollment,
                    Password = isq.Password
                };
                _studentContext.Student.Add(st);
                await _studentContext.SaveChangesAsync();
                return 0;
            }
            catch (SqlException ex)
            {
                return -1;
            }
        }
        public async Task<PromoteStudentResponse> PromoteStudentsAsync(PromoteStudentRequest promoteStudentRequest)
        {
            var listStudiesWithName = _studentContext.Studies.Where(s => s.Name == promoteStudentRequest.Studies).ToList();
            if (listStudiesWithName.Count() == 0)
            {
                return null;
            }
            var idStudy = listStudiesWithName[0].IdStudy;
            var existsStudiesSemester = _studentContext.Enrollment.Any(e => e.IdStudy == idStudy && e.Semester == promoteStudentRequest.Semester);

            if (!existsStudiesSemester)
            {
                return null;
            }
            var listStudents = (from s in _studentContext.Student
                                join e in _studentContext.Enrollment on s.IdEnrollment equals e.IdEnrollment
                                where e.IdStudy == idStudy && e.Semester == promoteStudentRequest.Semester
                                select s.IndexNumber).ToList();
            if (listStudents.Count() == 0)
            {
                return null;
            }
            int enrollmentId;
            var existsStudiesSemesterAdd1 = _studentContext.Enrollment.Any(e => e.IdStudy == idStudy && e.Semester == promoteStudentRequest.Semester + 1);
            DateTime startDateMade = DateTime.Now;
            if (!existsStudiesSemesterAdd1)
            {
                var m = new Enrollment
                {
                    Semester = promoteStudentRequest.Semester + 1,
                    IdStudy = idStudy,
                    StartDate = startDateMade,
                };
                _studentContext.Enrollment.Add(e);
                _studentContext.SaveChanges();
                enrollmentId = m.IdEnrollment;
            }
            else
            {
                var enrollments = _studentContext.Enrollment.Where(e => e.IdStudy == idStudy && e.Semester == promoteStudentRequest.Semester + 1).ToList();
                enrollmentId = enrollments[0].IdEnrollment;
                startDateMade = enrollments[0].StartDate;
            }
            foreach (var stu in listStudents)
            {
                var modifyStudent = new Student
                {
                    IndexNumber = stu,
                    IdEnrollment = enrollmentId
                };
                _studentContext.Attach(modifyStudent);
                _studentContext.Entry(modifyStudent).Property("IdEnrollment").IsModified = true;
                await _studentContext.SaveChangesAsync();
            }
            var psr = new PromoteStudentResponse
            {
                IdEnrollment = enrollmentId,
                IdStudy = idStudy,
                Semester = promoteStudentRequest.Semester,
                StartDate = startDateMade
            };
            return psr;
        }
        public async Task<EnrollmentStudentResponse> EnrollStudentAsync(EnrollmentStudenRequest request)
        {
            var studiesExists = _studentContext.Studies.Any(s => s.Name == request.Studies);
            if (!studiesExists)
            {
                return null;
            }
            var studiesList = _studentContext.Studies.Where(s => s.Name == request.Studies).ToList();
            int idStudy = studiesList[0].IdStudy;
            int enrollmentID = 1;
            var enrollmentExists = _studentContext.Enrollment.Any(e => e.IdStudy == idStudy && e.Semester == 1);
            if (enrollmentExists)
            {
                var enrollmentsList = _studentContext.Enrollment.Where(e => e.IdStudy == idStudy && e.Semester == 1).ToList();
                enrollmentID = enrollmentsList[0].IdEnrollment;
            }
            else
            {
                var m = new Enrollment
                {
                    Semester = 1,
                    IdStudy = idStudy,
                    StartDate = DateTime.Now
                };
                _studentContext.Enrollment.Add(e);
                _studentContext.SaveChanges();
                enrollmentID = m.IdEnrollment;
            }
            var exists = _studentContext.Student.Any(s => s.IndexNumber == request.IndexNumber);
            if (exists)
            {
                return null;
            }
            var isq = new InsertStudentRequest
            {
                IndexNumber = request.IndexNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate,
                IdEnrollment = enrollmentID,
                Password = request.Password
            };
            await AddStudentAsync(isq);
            var enrollStudentList = _studentContext.Enrollment.Where(e => e.IdEnrollment == enrollmentID).ToList();
            var esr = new EnrollmentStudentResponse
            {
                IdEnrollment = enrollStudentList[0].IdEnrollment,
                IdStudy = enrollStudentList[0].IdStudy,
                Semester = enrollStudentList[0].Semester,
                StartDate = enrollStudentList[0].StartDate
            };
            return esr;
        }
        public async Task<int> ModifyStudentAsync(ModifyStudentRequest usr)
        {
            var exists = _studentContext.Student.Any(s => s.IndexNumber == usr.IndexNumber);
            if (!exists)
            {
                return -1;
            }
            var st = new Student
            {
                IndexNumber = usr.IndexNumber,
                FirstName = usr.FirstName,
                LastName = usr.LastName,
                BirthDate = usr.BirthDate,
                IdEnrollment = usr.IdEnrollment,
                Password = usr.Password
            };
            _studentContext.Attach(st);
            _studentContext.Entry(st).State = EntityState.Modified;
            await _studentContext.SaveChangesAsync();
            return 0;
        }
        public async Task<int> RemoveStudentAsync(string index)
        {
            var exists = _studentContext.Student.Any(s => s.IndexNumber == index);
            if (!exists)
            {
                return -1;
            }
            var st = new Student
            {
                IndexNumber = index
            };
            _studentContext.Attach(st);
            _studentContext.Entry(st).State = EntityState.Deleted;
            await _studentContext.SaveChangesAsync();
            return 0;
        }
    }
}