using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Stocker.Extension;
using UnityEngine;

namespace Stocker.Framework
{
    public class Version<T> where T : class, ICloneable<T>, new()
    {
        private const int UNDO_MAX = 100;
        private T _instance = new T();
        private QueueStack<T> _undoStack = new QueueStack<T>();
        private QueueStack<T> _redoStack = new QueueStack<T>();
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

            _redoStack.PushBack(_instance.DeepClone());
            _instance = _undoStack.PopBack();
        }

        public void Redo()
        {
            if (_redoStack.Count == 0)
            {
                return;
            }
            
            _undoStack.PushBack(_instance.DeepClone());
            _instance = _redoStack.PopBack();
        }

        public void Commit()
        {
            var obj = _instance.DeepClone();
            _undoStack.PushBack(obj);
            if (_undoStack.Count == UNDO_MAX)
            {
                // 破棄
                _undoStack.PopFront();
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
            _undoStack.Clear();
            _redoStack.Clear();
            _instance = null;
        }
    }
}