using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp6
{
    public partial class Form1 : Form
    {
        private string connectionString = @"Server=.\SQLEXPRESS;Database=QiymetIdareetmeDB;Trusted_Connection=True;";
        private int selectedGradeId = -1;

        public Form1()
        {
            InitializeComponent();

            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            cmbStudent.SelectedIndexChanged += cmbStudent_SelectedIndexChanged;
            dgvGrades.CellClick += dgvGrades_CellClick;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dgvGrades.ReadOnly = true;
            dgvGrades.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGrades.AllowUserToAddRows = false;

            LoadStudents();
            LoadCourses();
            LoadGrades();
            CalculateAverage();
        }

        private void LoadStudents()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT StudentID, FullName FROM Students", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbStudent.DisplayMember = "FullName";
                cmbStudent.ValueMember = "StudentID";
                cmbStudent.DataSource = dt;
            }
        }

        private void LoadCourses()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT CourseID, CourseName FROM Courses", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbCourse.DisplayMember = "CourseName";
                cmbCourse.ValueMember = "CourseID";
                cmbCourse.DataSource = dt;
            }
        }

        private void LoadGrades()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        g.GradeID, 
                        g.StudentID, 
                        g.CourseID,
                        s.FullName AS [Tələbə], 
                        c.CourseName AS [Fənn], 
                        g.Score AS [Qiymət]
                    FROM Grades g
                    INNER JOIN Students s ON g.StudentID = s.StudentID
                    INNER JOIN Courses c ON g.CourseID = c.CourseID";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvGrades.DataSource = dt;

                if (dgvGrades.Columns["GradeID"] != null) dgvGrades.Columns["GradeID"].Visible = false;
                if (dgvGrades.Columns["StudentID"] != null) dgvGrades.Columns["StudentID"].Visible = false;
                if (dgvGrades.Columns["CourseID"] != null) dgvGrades.Columns["CourseID"].Visible = false;
            }
        }

        private void CalculateAverage()
        {
            if (cmbStudent.SelectedValue == null || !(cmbStudent.SelectedValue is int))
                return;

            int studentId = (int)cmbStudent.SelectedValue;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT AVG(Score * 1.0) FROM Grades WHERE StudentID = @StudentID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentID", studentId);

                conn.Open();
                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    lblAverage.Text = "Qiymət yoxdur";
                }
                else
                {
                    double avg = Convert.ToDouble(result);
                    lblAverage.Text = $"Orta bal: {avg:F2}";
                }
            }
        }

        private void cmbStudent_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalculateAverage();
        }

        private bool ValidateScore(out int score)
        {
            if (!int.TryParse(txtScore.Text.Trim(), out score) || score < 0 || score > 100)
            {
                MessageBox.Show("Zəhmət olmasa 0 ilə 100 arasında düzgün qiymət daxil edin.", "Xəta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (cmbStudent.SelectedValue == null || cmbCourse.SelectedValue == null) return;
            if (!ValidateScore(out int score)) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Grades (StudentID, CourseID, Score) VALUES (@StudentID, @CourseID, @Score)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentID", (int)cmbStudent.SelectedValue);
                cmd.Parameters.AddWithValue("@CourseID", (int)cmbCourse.SelectedValue);
                cmd.Parameters.AddWithValue("@Score", score);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            txtScore.Clear();
            selectedGradeId = -1;
            LoadGrades();
            CalculateAverage();
        }

        private void dgvGrades_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvGrades.Rows[e.RowIndex];
                selectedGradeId = Convert.ToInt32(row.Cells["GradeID"].Value);
                cmbStudent.SelectedValue = Convert.ToInt32(row.Cells["StudentID"].Value);
                cmbCourse.SelectedValue = Convert.ToInt32(row.Cells["CourseID"].Value);
                txtScore.Text = row.Cells["Qiymət"].Value.ToString();
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedGradeId == -1)
            {
                MessageBox.Show("Düzəliş etmək üçün cədvəldən bir sətir seçin.");
                return;
            }
            if (!ValidateScore(out int score)) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Grades SET StudentID = @StudentID, CourseID = @CourseID, Score = @Score WHERE GradeID = @GradeID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentID", (int)cmbStudent.SelectedValue);
                cmd.Parameters.AddWithValue("@CourseID", (int)cmbCourse.SelectedValue);
                cmd.Parameters.AddWithValue("@Score", score);
                cmd.Parameters.AddWithValue("@GradeID", selectedGradeId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            txtScore.Clear();
            selectedGradeId = -1;
            LoadGrades();
            CalculateAverage();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedGradeId == -1)
            {
                MessageBox.Show("Silmək üçün cədvəldən bir sətir seçin.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Grades WHERE GradeID = @GradeID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@GradeID", selectedGradeId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            txtScore.Clear();
            selectedGradeId = -1;
            LoadGrades();
            CalculateAverage();
        }

        // Dizayner xətası verməməsi üçün boş saxlanılıb
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}