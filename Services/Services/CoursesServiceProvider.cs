using System.Collections.Generic;
using System.Linq;
using CoursesAPI.Models;
using CoursesAPI.Services.DataAccess;
using CoursesAPI.Services.Exceptions;
using CoursesAPI.Services.Models.Entities;

namespace CoursesAPI.Services.Services
{
	public class CoursesServiceProvider
	{
		private readonly IUnitOfWork _uow;

		private readonly IRepository<CourseInstance> _courseInstances;
		private readonly IRepository<TeacherRegistration> _teacherRegistrations;
		private readonly IRepository<CourseTemplate> _courseTemplates; 
		private readonly IRepository<Person> _persons;

		public CoursesServiceProvider(IUnitOfWork uow)
		{
			_uow = uow;

			_courseInstances      = _uow.GetRepository<CourseInstance>();
			_courseTemplates      = _uow.GetRepository<CourseTemplate>();
			_teacherRegistrations = _uow.GetRepository<TeacherRegistration>();
			_persons              = _uow.GetRepository<Person>();
		}

		/// <summary>
		/// You should implement this function, such that all tests will pass.
		/// </summary>
		/// <param name="courseInstanceID">The ID of the course instance which the teacher will be registered to.</param>
		/// <param name="model">The data which indicates which person should be added as a teacher, and in what role.</param>
		/// <returns>Should return basic information about the person.</returns>
		public PersonDTO AddTeacherToCourse(int courseInstanceID, AddTeacherViewModel model)
		{
			// does the course exist
			var course = (from c in _courseInstances.All()
							where c.ID == courseInstanceID
							select c).SingleOrDefault();
			if(course == null)
			{
				throw new AppObjectNotFoundException();
			}

			// Does the person exist
			var person = (from c in _persons.All()
							where c.SSN == model.SSN
							select new PersonDTO {
								SSN = c.SSN,
								Name = c.Name
							}).SingleOrDefault();
			if(person == null)
			{
				throw new AppObjectNotFoundException();
			}

			// is there a MainTeacher in course
			int mainTeacher = (from c in _teacherRegistrations.All()
								where c.CourseInstanceID == courseInstanceID
								select c).Count();
			if(model.Type == TeacherType.MainTeacher && mainTeacher == 1)
			{
				throw new AppValidationException("COURSE_ALREADY_HAS_A_MAIN_TEACHER");
			}

			// Is the teacher already assigned to course
			var registration = (from c in _teacherRegistrations.All()
								where c.SSN == model.SSN
								&& c.CourseInstanceID == courseInstanceID
								select c).SingleOrDefault();
			if(registration != null && model.Type == TeacherType.AssistantTeacher)
			{
				throw new AppValidationException("PERSON_ALREADY_REGISTERED_TEACHER_IN_COURSE");
			}
			else
			{
				var teacher = (from c in _persons.All()
								where c.SSN == model.SSN
								select new TeacherRegistration {
									SSN = c.SSN,
									CourseInstanceID = courseInstanceID,
									Type = model.Type
								}).SingleOrDefault();
				_teacherRegistrations.Add(teacher);
				_uow.Save();
			}
			
			return person;
		}

		/// <summary>
		/// You should write tests for this function. You will also need to
		/// modify it, such that it will correctly return the name of the main
		/// teacher of each course.
		/// </summary>
		/// <param name="semester"></param>
		/// <returns></returns>
		public List<CourseInstanceDTO> GetCourseInstancesBySemester(string semester = null)
		{
			if (string.IsNullOrEmpty(semester))
			{
				semester = "20153";
			}

			var courses = (from c in _courseInstances.All()
				join ct in _courseTemplates.All() on c.CourseID equals ct.CourseID
				where c.SemesterID == semester
				select new CourseInstanceDTO
				{
					Name               = ct.Name,
					TemplateID         = ct.CourseID,
					CourseInstanceID   = c.ID,
					MainTeacher        = "" // Hint: it should not always return an empty string!
				}).ToList();
			
			return courses;
		}
	}
}
