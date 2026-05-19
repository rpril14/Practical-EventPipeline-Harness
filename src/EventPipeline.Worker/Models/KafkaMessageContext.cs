namespace EventPipeline.Worker.Models;

public record KafkaMessageContext(int Partition, long Offset);
