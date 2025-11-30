using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class MqttTicketLog
{
    public long Id { get; set; }

    public string? Topic { get; set; }

    public string? Message { get; set; }

    public string? ResponseTopic { get; set; }

    public string? ResponseMessage { get; set; }

    public DateTime? RecordDate { get; set; }

    public string? MachineName { get; set; }
}
