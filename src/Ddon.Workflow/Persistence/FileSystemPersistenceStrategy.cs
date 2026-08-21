using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ddon.Workflow.Persistence
{
    /// <summary>
    /// 基于文件系统的持久化策略，使用JSON序列化
    /// </summary>
    public class FileSystemPersistenceStrategy : IWorkflowPersistenceStrategy
    {
        private readonly string _storagePath;
        private readonly ILogger<FileSystemPersistenceStrategy> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public FileSystemPersistenceStrategy(
            string storagePath,
            ILogger<FileSystemPersistenceStrategy> logger)
        {
            _storagePath = storagePath ??
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "workflow_checkpoints");
            _logger = logger;

            // 确保目录存在
            Directory.CreateDirectory(_storagePath);

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task SaveCheckpointAsync(
            IWorkflowCheckpoint checkpoint,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var filePath = GetCheckpointFilePath(checkpoint.WorkflowId);
                var json = JsonSerializer.Serialize(checkpoint, _jsonOptions);

                await Task.Run(() => File.WriteAllText(filePath, json), cancellationToken);

                _logger.LogInformation(
                    $"[持久化] 工作流 '{checkpoint.WorkflowName}' (ID: {checkpoint.WorkflowId}) 的检查点已保存，当前步骤: {checkpoint.CurrentStepIndex}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    $"[持久化] 保存工作流 {checkpoint.WorkflowId} 的检查点失败");
                throw;
            }
        }

        public async Task<IWorkflowCheckpoint> LoadCheckpointAsync(
            string workflowId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var filePath = GetCheckpointFilePath(workflowId);

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning($"[持久化] 工作流 {workflowId} 的检查点不存在");
                    return null;
                }

                var json = await Task.Run(() => File.ReadAllText(filePath), cancellationToken);
                var checkpoint = JsonSerializer.Deserialize<WorkflowCheckpoint>(json, _jsonOptions);

                _logger.LogInformation(
                    $"[持久化] 工作流 '{checkpoint.WorkflowName}' 的检查点已加载，当前步骤: {checkpoint.CurrentStepIndex}");

                return checkpoint;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    $"[持久化] 加载工作流 {workflowId} 的检查点失败");
                throw;
            }
        }

        public async Task<IWorkflowCheckpoint[]> GetAllCheckpointsAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!Directory.Exists(_storagePath))
                    return Array.Empty<IWorkflowCheckpoint>();

                var files = Directory.GetFiles(_storagePath, "*.json");
                var checkpoints = new List<IWorkflowCheckpoint>();

                foreach (var file in files)
                {
                    try
                    {
                        var json = await Task.Run(() => File.ReadAllText(file), cancellationToken);
                        var checkpoint = JsonSerializer.Deserialize<WorkflowCheckpoint>(json, _jsonOptions);
                        checkpoints.Add(checkpoint);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"[持久化] 反序列化检查点失败: {file}");
                    }
                }

                return checkpoints.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[持久化] 获取所有检查点失败");
                throw;
            }
        }

        public async Task DeleteCheckpointAsync(
            string workflowId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var filePath = GetCheckpointFilePath(workflowId);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation($"[持久化] 工作流 {workflowId} 的检查点已删除");
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[持久化] 删除工作流 {workflowId} 的检查点失败");
                throw;
            }
        }

        public Task<bool> CheckpointExistsAsync(
            string workflowId,
            CancellationToken cancellationToken = default)
        {
            var filePath = GetCheckpointFilePath(workflowId);
            return Task.FromResult(File.Exists(filePath));
        }

        private string GetCheckpointFilePath(string workflowId)
        {
            return Path.Combine(_storagePath, $"{workflowId}.checkpoint.json");
        }
    }
}
