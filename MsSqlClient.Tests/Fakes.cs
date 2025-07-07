using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace MsSqlClient.Tests;

// Fake implementation of DbConnection for testing purposes.
public class FakeDbConnection : DbConnection
{
    private ConnectionState _state;
    public override string ConnectionString { get; set; } = "";
    public override string Database => "FakeDb";
    public override string DataSource => "FakeServer";
    public override string ServerVersion => "1.0";
    public override ConnectionState State => _state;

    [AllowNull]
    public override ISite? Site { get; set; }

    public override void ChangeDatabase(string databaseName) { }
    public override void Close() => _state = ConnectionState.Closed;
    public override void Open() => _state = ConnectionState.Open;
    protected override DbCommand CreateDbCommand() => new FakeDbCommand { Connection = this };
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => new FakeDbTransaction(this, isolationLevel);
}

// Fake implementation of DbTransaction for testing purposes.
public class FakeDbTransaction : DbTransaction
{
    public override IsolationLevel IsolationLevel { get; }
    protected override DbConnection DbConnection { get; }

    public FakeDbTransaction(DbConnection connection, IsolationLevel isolationLevel)
    {
        DbConnection = connection;
        IsolationLevel = isolationLevel;
    }

    public override void Commit() { }
    public override void Rollback() { }
}

// Fake implementation of DbCommand for testing purposes.
public class FakeDbCommand : DbCommand
{
    public override string CommandText { get; set; } = "";
    public override int CommandTimeout { get; set; } = 30;
    public override CommandType CommandType { get; set; } = CommandType.Text;
    public override bool DesignTimeVisible { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    protected override DbConnection? DbConnection { get; set; }
    protected override DbParameterCollection DbParameterCollection { get; } = new FakeDbParameterCollection();
    protected override DbTransaction? DbTransaction { get; set; }

    public Func<CancellationToken, Task<int>>? ExecuteNonQueryAsyncHandler { get; set; }
    public Func<CancellationToken, Task<object?>>? ExecuteScalarAsyncHandler { get; set; }
    public Func<CancellationToken, Task<DbDataReader>>? ExecuteReaderAsyncHandler { get; set; }

    public override void Cancel() { }
    public override int ExecuteNonQuery() => ExecuteNonQueryAsync(CancellationToken.None).Result;
    public override object? ExecuteScalar() => ExecuteScalarAsync(CancellationToken.None).Result;
    public override void Prepare() { }

    public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        return ExecuteNonQueryAsyncHandler?.Invoke(cancellationToken) ?? Task.FromResult(0);
    }

    public override Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        return ExecuteScalarAsyncHandler?.Invoke(cancellationToken) ?? Task.FromResult<object?>(null);
    }

    protected override DbParameter CreateDbParameter() => new FakeDbParameter();
    protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
    {
        return ExecuteReaderAsyncHandler?.Invoke(cancellationToken) ?? Task.FromResult<DbDataReader>(new FakeDbDataReader());
    }
    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => ExecuteDbDataReaderAsync(behavior, CancellationToken.None).Result;
}

// Fake implementation of DbParameterCollection for testing purposes.
public class FakeDbParameterCollection : DbParameterCollection
{
    private readonly List<DbParameter> _parameters = new();
    public override int Count => _parameters.Count;
    public override object SyncRoot => this;

    public override int Add(object value) { _parameters.Add((DbParameter)value); return _parameters.Count - 1; }
    public override void AddRange(Array values) { foreach (var v in values) Add(v); }
    public override void Clear() => _parameters.Clear();
    public override bool Contains(string value) => _parameters.Exists(p => p.ParameterName == value);
    public override bool Contains(object? value) => _parameters.Contains((DbParameter)value!);
    public override void CopyTo(Array array, int index) => throw new NotImplementedException();
    public override IEnumerator GetEnumerator() => _parameters.GetEnumerator();
    public override int IndexOf(string parameterName) => _parameters.FindIndex(p => p.ParameterName == parameterName);
    public override int IndexOf(object? value) => _parameters.IndexOf((DbParameter)value!);
    public override void Insert(int index, object? value) => _parameters.Insert(index, (DbParameter)value!);
    public override void Remove(object? value) => _parameters.Remove((DbParameter)value!);
    public override void RemoveAt(string parameterName) => _parameters.RemoveAll(p => p.ParameterName == parameterName);
    public override void RemoveAt(int index) => _parameters.RemoveAt(index);
    protected override DbParameter GetParameter(string parameterName) => _parameters.Find(p => p.ParameterName == parameterName)!;
    protected override DbParameter GetParameter(int index) => _parameters[index];
    protected override void SetParameter(string parameterName, DbParameter value) { var index = IndexOf(parameterName); if (index >= 0) _parameters[index] = value; else Add(value); }
    protected override void SetParameter(int index, DbParameter value) => _parameters[index] = value;
}

// Fake implementation of DbParameter for testing purposes.
public class FakeDbParameter : DbParameter
{
    public override DbType DbType { get; set; }
    public override ParameterDirection Direction { get; set; }
    public override bool IsNullable { get; set; }
    public override string ParameterName { get; set; } = "";
    public override string SourceColumn { get; set; } = "";
    public override object? Value { get; set; }
    public override bool SourceColumnNullMapping { get; set; }
    public override int Size { get; set; }
    public override void ResetDbType() { }
}

// Fake implementation of DbDataReader for testing purposes.
public class FakeDbDataReader : DbDataReader
{
    private readonly List<object[]> _rows;
    private int _currentRow = -1;

    public FakeDbDataReader(List<object[]>? rows = null)
    {
        _rows = rows ?? new List<object[]>();
    }

    public override object GetValue(int ordinal) => _rows[_currentRow][ordinal];
    public override bool Read() { _currentRow++; return _currentRow < _rows.Count; }
    public override Task<bool> ReadAsync(CancellationToken cancellationToken) => Task.FromResult(Read());
    public override bool NextResult() => false;
    public override Task<bool> NextResultAsync(CancellationToken cancellationToken) => Task.FromResult(false);

    // Implement all other abstract members from DbDataReader
    public override bool HasRows => _rows.Count > 0;
    public override int Depth => 0;
    public override bool IsClosed => false;
    public override int RecordsAffected => 0;
    public override int FieldCount => _rows.Count > 0 ? _rows[0].Length : 0;
    public override object this[int ordinal] => GetValue(ordinal);
    public override object this[string name] => GetValue(GetOrdinal(name));

    public override bool GetBoolean(int ordinal) => (bool)GetValue(ordinal);
    public override byte GetByte(int ordinal) => (byte)GetValue(ordinal);
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) => throw new NotImplementedException();
    public override char GetChar(int ordinal) => (char)GetValue(ordinal);
    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) => throw new NotImplementedException();
    public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;
    public override DateTime GetDateTime(int ordinal) => (DateTime)GetValue(ordinal);
    public override decimal GetDecimal(int ordinal) => (decimal)GetValue(ordinal);
    public override double GetDouble(int ordinal) => (double)GetValue(ordinal);
    public override IEnumerator GetEnumerator() => _rows.GetEnumerator();
    public override Type GetFieldType(int ordinal) => GetValue(ordinal).GetType();
    public override float GetFloat(int ordinal) => (float)GetValue(ordinal);
    public override Guid GetGuid(int ordinal) => (Guid)GetValue(ordinal);
    public override short GetInt16(int ordinal) => (short)GetValue(ordinal);
    public override int GetInt32(int ordinal) => (int)GetValue(ordinal);
    public override long GetInt64(int ordinal) => (long)GetValue(ordinal);
    public override string GetName(int ordinal) => $"Column{ordinal}";
    public override int GetOrdinal(string name) => int.Parse(name.Replace("Column", ""));
    public override string GetString(int ordinal) => (string)GetValue(ordinal);
    public override int GetValues(object[] values) { _rows[_currentRow].CopyTo(values, 0); return _rows[_currentRow].Length; }
    public override bool IsDBNull(int ordinal) => GetValue(ordinal) == DBNull.Value;
}
