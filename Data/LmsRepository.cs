using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LmsApp.API.Helpers;
using LmsApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LmsApp.API.Data
{
    public class LmsRepository : ILmsRepository
    {
        private readonly DataContext _context;
        public LmsRepository(DataContext context)
        {
            _context = context;

        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(u => u.UserId == userId)
                .FirstOrDefaultAsync(p => p.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);

            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(u => u.Id == id);

            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users =  _context.Users.Include(p => p.Photos)
                .OrderByDescending(u => u.LastActive).AsQueryable();

            // Filter out current user from list of returned user
            users = users.Where(u => u.Id != userParams.UserId);

            // Only return gender of user specified in userParams
            // If user is male, return females
            // If user is female, return males
            users = users.Where(u => u.Gender == userParams.Gender);

            // Filter users based on age - default to 18 - 99 if not specified 
            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }

            // Filter users based on when account created - default to last active
            // if nothing supplied
            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created": 
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }
            
            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}