using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ddon.Repositiry.Dapper
{
    public abstract class QueryBase<TTenantKey>
    {
        private bool _isResolveCondition;

        /// <summary>
        /// 租户 Id
        /// </summary>
        public TTenantKey? TenantId { get; set; }

        /// <summary>
        /// 是否只查询租户数据
        /// </summary>
        private readonly bool _isQueryTenant;

        /// <summary>
        /// 是否只查询软删除数据
        /// </summary>
        private readonly bool _isNotQueryDelete;

        /// <summary>
        /// WHERE 查询条件 "AND模式"
        /// </summary>
        private readonly List<QueryWhereInfo> _wheres = new();

        /// <summary>
        /// 排序字段
        /// </summary>
        private readonly List<string> _sorts = new();


        public QueryBase(TTenantKey? tenantId = default, bool isNotQueryDelete = true)
        {
            TenantId = tenantId;

            _isQueryTenant = tenantId is not null;
            _isNotQueryDelete = isNotQueryDelete;
        }

        /// <summary>
        /// SELECT 部分的 SQL
        /// </summary>
        /// <returns></returns>
        protected abstract string SelectSql();

        /// <summary>
        /// FORM 部分的 SQL
        /// </summary>
        /// <returns></returns>
        protected abstract string FromSql();

        /// <summary>
        /// JOIN 部分的 SQL
        /// </summary>
        /// <returns></returns>
        protected abstract string JoinSql();

        /// <summary>
        /// WHERE 条件在此方法内进行添加
        /// </summary>
        /// <returns></returns>
        protected abstract void WhereSql();

        /// <summary>
        /// 排序条件在此方法内进行添加
        /// </summary>
        /// <returns></returns>
        protected abstract void SortSql();


        protected virtual void AddWhere(string whereString, AndOr andOr = AndOr.And)
        {
            _wheres.Add(new QueryWhereInfo(whereString, andOr));
        }


        protected void AddSort(string sortString, SortType sortType = SortType.Asc)
        {
            var sort = $"{sortString} {sortType}";
            if (!_sorts.Contains(sort))
            {
                _sorts.Add(sort);
            }
        }

        /// <summary>
        /// 获取完整的查询 SQL
        /// </summary>
        /// <returns></returns>
        public string BuildCompleteSql()
        {
            InitCondition();
            StringBuilder sqlBuilder = new();
            sqlBuilder.Append(BuildSelectSql()).Append(BuildFromSql()).Append(BuildWhereSql());

            if (_sorts.Any())
            {
                sqlBuilder.Append("ORDER BY ").Append(string.Join(",", _sorts)).Append(Environment.NewLine);
            }
            return sqlBuilder.ToString();
        }

        public string BuildSelectSql()
        {
            StringBuilder sqlBuilder = new();
            sqlBuilder.Append(SelectSql()).Append(Environment.NewLine);
            return sqlBuilder.ToString();
        }

        public string BuildFromSql()
        {
            var fromSql = FromSql();
            if (fromSql.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            StringBuilder sqlBuilder = new();
            sqlBuilder.Append(FromSql()).Append(Environment.NewLine);

            var joinSql = JoinSql();
            if (!joinSql.IsNullOrWhiteSpace())
            {
                sqlBuilder.Append(JoinSql()).Append(Environment.NewLine);
            }

            return sqlBuilder.ToString();
        }

        public string BuildWhereSql()
        {
            InitCondition();
            StringBuilder sqlBuilder = new();

            var whereSql = string.Join(string.Empty, _wheres.Select(x => $" {x.AndOr} {x.WhereSql}"));
            sqlBuilder.Append("WHERE 1 = 1").Append(whereSql).Append(Environment.NewLine);
            return sqlBuilder.ToString();
        }

        protected void InitCondition()
        {
            if (_isResolveCondition) return;
            lock (this)
            {
                if (!_isResolveCondition)
                {
                    WhereSql();
                    SortSql();

                    if (_isNotQueryDelete)
                    {
                        AddWhere("IsDeleted = 0", AndOr.And);
                    }

                    if (_isQueryTenant)
                    {
                        AddWhere("TenantId = @TenantId", AndOr.And);
                    }

                    _isResolveCondition = true;
                }
            }
        }

    }

    public abstract class QueryBase : QueryBase<Guid?>
    {
        public QueryBase(Guid? tenantId = default, bool isNotQueryDelete = true) :
            base(tenantId, isNotQueryDelete)
        {

        }
    }
}
