using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {

        private readonly ApplicationDbContext _db;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
        }
        public ICategoryRepository CategoryRepository => _categoryRepository ?? new CategoryRepository(_db);
        public IProductRepository ProductRepository => _productRepository ?? new ProductRepository(_db);

        public void Save() => _db.SaveChanges();
        public void Detach(object entity)
		{
			_db.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
		}
	}
}
