using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using StudentExercisesMVC.Models;
using StudentExercisesMVC.Models.ViewModels;

namespace StudentExercisesMVC.Controllers
{
    public class StudentsController : Controller
    {
        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // GET: Students
        public async Task<ActionResult> Index()
        {
            var students = await List( -1 );
            return View(students);
        }

        // GET: Students/Details/5
        public async Task<ActionResult> Details([FromRoute] int id)
        {
            if ( id > 0 )
            {
                var students = await List(id);
                return View(students[0]);
            }
            else
            {
                return View();
            }
        }

        private async Task<List<Student>> List(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.Id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId StudentCohortId,
                                               c.Id CohortId, c.Name CohortName,
                                               e.Id ExerciseId, e.Name ExerciseName, e.Language ExerciseLanguage
                                        FROM Student s
                                        LEFT JOIN Cohort c 
                                            ON s.CohortId = c.Id
                                        LEFT JOIN StudentExercise se 
                                            ON s.Id = se.StudentId
                                        LEFT JOIN Exercise e 
                                            ON se.ExerciseId = e.Id ";

                    if ( id > 0 )
                    {
                        cmd.CommandText += @" WHERE s.Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                    }

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<Student> students = new List<Student>();

                    int StudentIdOrdinal = reader.GetOrdinal("Id");
                    int FirstNameOrdinal = reader.GetOrdinal("FirstName");
                    int LastNameOrdinal = reader.GetOrdinal("LastName");
                    int SlackHandleOrdinal = reader.GetOrdinal("SlackHandle");
                    int StudentCohortIdOrdinal = reader.GetOrdinal("StudentCohortId");
                    int CohortIdOrdinal = reader.GetOrdinal("CohortId");
                    int CohortNameOrdinal = reader.GetOrdinal("CohortName");
                    int ExerciseIdOrdinal = reader.GetOrdinal("ExerciseId");
                    int ExerciseNameOrdinal = reader.GetOrdinal("ExerciseName");
                    int ExerciseLanguageOrdinal = reader.GetOrdinal("ExerciseLanguage");

                    while (reader.Read())
                    {
                        var studentId = reader.GetInt32(StudentIdOrdinal);

                        var studentAlreadyAdded = students.FirstOrDefault(s => s.Id == studentId);

                        var hasExercise = !reader.IsDBNull(ExerciseIdOrdinal);

                        if (studentAlreadyAdded == null)
                        {
                            Student student = new Student
                            {
                                Id = reader.GetInt32(StudentIdOrdinal),
                                //Id = studentId,
                                FirstName = reader.GetString(FirstNameOrdinal),
                                LastName = reader.GetString(LastNameOrdinal),
                                SlackHandle = reader.GetString(SlackHandleOrdinal),
                                CohortId = reader.GetInt32(StudentCohortIdOrdinal),
                                Cohort = new Cohort()
                                {
                                    Id = reader.GetInt32(CohortIdOrdinal),
                                    Name = reader.GetString(CohortNameOrdinal)
                                }
                            };
                            students.Add(student);

                            if (hasExercise)
                            {
                                Exercise exercise = new Exercise
                                {
                                    Id = reader.GetInt32(ExerciseIdOrdinal),
                                    Name = reader.GetString(ExerciseNameOrdinal),
                                    Language = reader.GetString(ExerciseLanguageOrdinal)
                                };
                                student.StudentsExercises.Add(exercise);
                            }
                        }
                        else
                        {
                            if (hasExercise)
                            {
                                studentAlreadyAdded.StudentsExercises.Add(new Exercise()
                                {
                                    Id = reader.GetInt32(ExerciseIdOrdinal),
                                    Name = reader.GetString(ExerciseNameOrdinal),
                                    Language = reader.GetString(ExerciseLanguageOrdinal)
                                });
                            }
                        }
                    }
                    reader.Close();

                    return students;
                }
            }
        }


        // GET: Students/Create
        public ActionResult Create()
        {
            var student = new Student();

            var cohorts = GetCohorts().Select( d => new SelectListItem
            {
                Text = d.Name,
                Value = d.Id.ToString()
            }).ToList();

            var viewModel = new StudentViewModel
            {
                Student = new Student(),
                Cohorts = cohorts
            };

            return View(viewModel);
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Student student)
        {
            try
            {
                // TODO: Add insert logic here
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Student (FirstName, LastName, SlackHandle, CohortId)
                                            VALUES (@firstName, @lastName, @slackHandle, @cohortId)";

                        cmd.Parameters.Add(new SqlParameter("@firstName", student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", student.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@departmentId", student.CohortId));

                        cmd.ExecuteNonQuery();
                    }
                }

                // RedirectToAction("Index");
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Students/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var cohorts = GetCohorts().Select(d => new SelectListItem
            {
                Text = d.Name,
                Value = d.Id.ToString()
            }).ToList();

            if (id > 0)
            {
                var students = await List(id);

                var viewModel = new StudentViewModel
                {
                    Student = students[0],
                    Cohorts = cohorts
                };

                return View(viewModel);
            }
            else
            {
                return NotFound();
            }
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Student student)
        {
            try
            {
                // TODO: Add update logic here
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Student
                                            SET FirstName = @firstName, 
                                                LastName = @lastName, 
                                                SlackHandle = @slackHandle,                                                
                                                CohortId = @cohortId
                                            WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@firstName", student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", student.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", student.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();

                        /*if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        return BadRequest($"No Employee with Id of {id}");*/
                    }
                }

                return RedirectToAction( nameof(Index) );
            }
            catch (Exception ex)
            {
                //return View();
                return RedirectToAction( nameof(Edit), new { id = id } );
            }
        }

        // GET: Students/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var cohorts = GetCohorts().Select(d => new SelectListItem
            {
                Text = d.Name,
                Value = d.Id.ToString()
            }).ToList();

            if (id > 0)
            {
                var students = await List(id);

                var viewModel = new StudentViewModel
                {
                    Student = students[0],
                    Cohorts = cohorts
                };

                return View(viewModel);
            }
            else
            {
                return NotFound();
            }
        }

        // POST: Students/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        
        private List<Cohort> GetCohorts()
        {
            using ( SqlConnection conn = Connection )
            {
                conn.Open();
                using ( SqlCommand cmd = conn.CreateCommand() )
                {
                    cmd.CommandText = @"SELECT Id, Name FROM Cohort";

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Cohort> cohorts = new List<Cohort>();

                    int IdOrdinal = reader.GetOrdinal("Id");
                    int NameOrdinal = reader.GetOrdinal("Name");

                    while ( reader.Read() )
                    {
                        cohorts.Add( new Cohort
                        {
                            Id = reader.GetInt32(IdOrdinal),
                            Name = reader.GetString(NameOrdinal)
                        });
                    }

                    reader.Close();

                    return cohorts;
                }
            }
        }
    }
}