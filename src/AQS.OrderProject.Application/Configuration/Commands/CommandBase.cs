using System;

namespace AQS.OrderProject.Application.Configuration.Commands
{
    public class CommandBase : ICommand
    {
        public Guid Id { get; }

        public string UserName { get; set; }

        public CommandBase()
        {
            this.Id = Guid.NewGuid();
        }

        protected CommandBase(Guid id)
        {
            this.Id = id;
        }
    }

    public abstract class CommandBase<TResult> : ICommand<TResult>
    {
        public Guid Id { get; }

        public string UserName { get; set; }

        protected CommandBase()
        {
            this.Id = Guid.NewGuid();
        }

        protected CommandBase(Guid id)
        {
            this.Id = id;
        }
    }
}