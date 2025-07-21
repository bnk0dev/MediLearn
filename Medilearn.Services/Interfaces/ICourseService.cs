using Medilearn.Models.DTOs;

namespace Medilearn.Services.Interfaces
{
    public interface ICourseService
    {
        // Belirli bir eğitmene ait kursları döndürür
        Task<IEnumerable<CourseDto>> GetCoursesByInstructorAsync(string instructorTcNo);

        // Sistemdeki tüm kursları döndürür
        Task<IEnumerable<CourseDto>> GetAllCoursesAsync();

        // Yeni bir kurs oluşturur
        Task<bool> CreateCourseAsync(CourseDto courseDto);

        // Var olan bir kursu günceller
        Task<bool> UpdateCourseAsync(CourseDto courseDto);

        // Eğitmen TC numarasıyla birlikte yeni bir kurs ekler
        Task<bool> AddCourseAsync(CourseDto courseDto, string instructorTcNo);

        // Kurs ID'sine göre kurs detayını getirir
        Task<CourseDto> GetCourseByIdAsync(int courseId);

        // Belirli bir eğitmenin oluşturduğu en son kursu getirir
        Task<CourseDto> GetLatestCourseByInstructor(string instructorTcNo);

        // Kursu ekleyip kursun ID değerini döndürür
        Task<int> AddCourseAndReturnIdAsync(CourseDto dto, string instructorTcNo);

        // Kursu ID'sine göre siler
        Task<bool> DeleteCourseAsync(int courseId);

        // Belirli bir personele ait kayıtlı olduğu kursları getirir
        Task<List<CourseDto>> GetCoursesByPersonnelAsync(string personnelTcNo);

        // Sistemdeki toplam kurs sayısını döndürür
        Task<int> GetTotalCoursesAsync();

        // Son oluşturulan kursları döndürür
        Task<IEnumerable<CourseDto>> GetRecentCoursesAsync();
    }
}
