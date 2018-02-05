using Microsoft.AspNetCore.Mvc;
using ChatApp.Data;

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

        protected T CreateModel<T, F>(T entity, F from, string keys) where T : class
        {
            return CreateModel(MergeModel(from: from, to: entity, keys: keys));
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

        protected T UpdateModel<F, T>(F from, T to, string keys) where T : EntityBase
        {
            MergeModel(from: from, to: to, keys: keys);
            return SetUpdatedProperties(to);
        }

        protected T MergeModel<F, T>(F from, T to, string keys)
        {
            var typeT = typeof(T);
            var typeF = typeof(F);
            foreach (var propertyName in keys.Split(new char[] { ',' }))
            {
                typeT.GetProperty(propertyName)
                    .SetValue(to, typeF.GetProperty(propertyName).GetValue(from));
            }
            return to;
        }
    }
}
