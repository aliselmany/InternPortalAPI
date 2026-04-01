using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace InternPortal.Application.Dtos
{
    public record UpdateUserDto
    {

      [DefaultValue("")]  
      public string? Name { get; set; }

      [DefaultValue("")]
      public string? Surname {  get; set; }

      [DefaultValue("")]
      public string? Email { get; set; }

      [DefaultValue("")]
      public string? Password { get; set;  }

    }
}
