using Microsoft.EntityFrameworkCore.Storage;
using N5.Permissions.Domain.Interfaces;
using N5.Permissions.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N5.Permissions.Infrastructure.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PermissionsDbContext _context;
        private IDbContextTransaction? _transaction;
        private IPermissionRepository? _permissions;
        private IPermissionTypeRepository? _permissionTypes;

        public UnitOfWork(PermissionsDbContext context)
        {
            _context = context;
        }

        public IPermissionRepository Permissions
        {
            get { return _permissions ??= new PermissionRepository(_context); }
        }

        public IPermissionTypeRepository PermissionTypes
        {
            get { return _permissionTypes ??= new PermissionTypeRepository(_context); }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
