﻿// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.Data.Providers
{
	public class DataFile : IExternalDataFile
	{
		IList<Field> _fields;
		IList<Record> _records;

		private readonly Byte[] byteCodes1251 = new Byte[] { 0x81, 0x83, 0xA1, 0xA2, 0xA5, 0xA8, 0xAA, 0xAF, 0xB2, 0xB3, 0xB4, 0xBA, 0xBF };

		public DateTime LastModifedDate { get; set; }

		public DataFile()
		{
			_fields = new List<Field>();
			_records = new List<Record>();
			LastModifedDate = DateTime.Today;
			Encoding = null; // automatic
		}

		public Encoding Encoding { get; set; }

		public Encoding FindDecoding(Byte[] chars)
		{
			if (Encoding != null)
				return Encoding;
			Int32 countASCII = 0;
			Int32 count866 = 0;
			Int32 count1251 = 0;
			for (Int32 i=0; i<chars.Length; i++)
			{
				Byte ch = chars[i];
				if (ch < 0x80) {
					countASCII += 1;
					continue;
				}
				if (ch >= 0xC0 && ch <= 0xFF || Array.IndexOf(byteCodes1251, ch) != -1)
					count1251 += 1;
				if (ch >= 0x80 && ch <= 0xAF || ch >= 0xE0 && ch <= 0xF7)
					count866 += 1;
			}
			if (countASCII == chars.Length)
				return Encoding.ASCII;
			count1251 += countASCII;
			count866 += countASCII;
			var totalCount = chars.Length;
			if (count1251 == totalCount && count866 < totalCount)
			{
				this.Encoding = Encoding.GetEncoding(1251);
				return this.Encoding;
			} else if (count866 == totalCount && count1251 < totalCount)
			{
				this.Encoding = Encoding.GetEncoding(866);
				return this.Encoding;
			}

			return Encoding.ASCII;
		}

		public Int32 FieldCount => _fields.Count;
		public Int32 NumRecords => _records.Count;

		public Field CreateField()
		{
			var f = new Field();
			_fields.Add(f);
			return f;
		}

		public Field GetField(Int32 index)
		{
			if (index < 0 || index >= _fields.Count)
				throw new InvalidOperationException();
			return _fields[index];
		}
		public IEnumerable<Field> Fields => _fields;

		private IDictionary<String, Int32> _fieldMap;

		internal void MapFields()
		{
			_fieldMap = new Dictionary<String, Int32>();
			for (Int32 f = 0; f < _fields.Count; f++)
				_fieldMap.Add(_fields[f].Name, f);
		}

		public Record CreateRecord()
		{
			var r = new Record(_fieldMap);
			_records.Add(r);
			return r;
		}

		public Record GetRecord(Int32 index)
		{
			if (index < 0 || index >= _records.Count)
				throw new InvalidOperationException();
			return _records[index];
		}
		public IEnumerable<IExternalDataRecord> Records => _records;

	}
}
