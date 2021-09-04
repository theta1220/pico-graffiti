using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Stocker.Extension;
using UnityEngine;

namespace Stocker.Framework
{
    public class Version<T> where T : class, new()
    {
        private const int UNDO_MAX = 100;
        private T _instance = new T();
        private QueueStack<MemoryStream> _undoStack = new QueueStack<MemoryStream>();
        private QueueStack<MemoryStream> _redoStack = new QueueStack<MemoryStream>();
        public T Instance => _instance;
        
        ~Version()
        {
            Clear();
        }
        
        public void Undo()
        {
            if (_undoStack.Count == 0)
            {
                return;
            }

            _redoStack.PushBack(_instance.Serialize());
            var obj = _undoStack.PopBack();
            obj.Seek(0, SeekOrigin.Begin);
            _instance = obj.Deserialize<T>();
        }

        public void Redo()
        {
            if (_redoStack.Count == 0)
            {
                return;
            }
            
            _undoStack.PushBack(_instance.Serialize());
            var obj = _redoStack.PopBack();
            obj.Seek(0, SeekOrigin.Begin);
            _instance = obj.Deserialize<T>();
        }

        public void Commit()
        {
            var obj = _instance.Serialize();
            _undoStack.PushBack(obj);
            if (_undoStack.Count == UNDO_MAX)
            {
                // 破棄
                var mem = _undoStack.PopFront();
                mem.Dispose();
            }
            
            foreach (var memoryStream in _redoStack)
            {
                memoryStream.Dispose();
            }
            _redoStack.Clear();
        }

        public void SetInstance(T obj)
        {
            if (obj == null) return;
            Clear();
            _instance = obj;
        }

        public void Clear()
        {
            foreach (var memoryStream in _undoStack)
            {
                memoryStream.Dispose();
            }

            foreach (var memoryStream in _redoStack)
            {
                memoryStream.Dispose();
            }
            _undoStack.Clear();
            _redoStack.Clear();
            _instance = null;
        }
    }
}