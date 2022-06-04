// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 2.0.
// Copyright (c) 2007-2020 VMware, Inc.

namespace rabbitmq_backgroundservice;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class Worker : BackgroundService
{
    private readonly TimeSpan _stoppingCheckInterval = TimeSpan.FromSeconds(5);
    private readonly ILogger<Worker> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly EventingBasicConsumer _consumer;
    private readonly string _consumerTag;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _consumer = new EventingBasicConsumer(_channel);
        _consumer.Received += ReceivedHandler;

        _consumerTag = _channel.BasicConsume("MyQueue", false, _consumer);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (_connection)
        using (_channel)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(_stoppingCheckInterval, stoppingToken);
            }

            _logger.LogInformation("Worker STOPPING at: {time}", DateTimeOffset.Now);
            _channel.BasicCancel(_consumerTag);
        }
    }

    private void ReceivedHandler(object? sender, BasicDeliverEventArgs ea)
    {
        var tag = ea.DeliveryTag;
        _logger.LogInformation("Received message. tag: {tag}  at: {time}", tag, DateTimeOffset.Now);
        _channel.BasicAck(tag, false);
    }
}
