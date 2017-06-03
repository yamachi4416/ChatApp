using ChatApp.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ChatApp.Controllers
{
    public partial class AppControllerBase : Controller
    {
        protected T CreateModel<T>(T entity) where T : class
        {
            if (entity is EntityBase)
            {
                SetCreatedProperties(entity as EntityBase);
            }

            _db.Add(entity);

            return entity;
        }

        protected T SetCreatedProperties<T>(T entity) where T : EntityBase
        {
            SetUpdatedProperties(entity);

            entity.CreatedDate = entity.UpdatedDate;
            entity.CreatedById = entity.UpdatedById;

            return entity;
        }

        protected T SetUpdatedProperties<T>(T entity) where T : EntityBase
        {
            entity.UpdatedDate = DateTimeOffsetNow;
            entity.UpdatedById = GetCurrentUserId();

            return entity;
        }

        protected T UpdateModel<F, T>(F from, T to, IEnumerable<string> keys = null) where T : EntityBase
        {
            MergeModel(from: from, to: to, keys: keys);
            return SetUpdatedProperties(to);
        }

        protected T MergeModel<F, T>(F from, T to, IEnumerable<string> keys = null)
        {
            var typeT = typeof(T);
            var typeF = typeof(F);
            foreach (var propertyName in (keys ?? ModelState.Keys))
            {
                typeT.GetProperty(propertyName)
                    .SetValue(to, typeF.GetProperty(propertyName).GetValue(from));
            }
            return to;
        }
    }
}
