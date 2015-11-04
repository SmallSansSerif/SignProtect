using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace SignProtect
{
    [ApiVersion(1, 22)]
    public class SignProtect : TerrariaPlugin
    {
        public override Version Version
        {
            get { return new Version("1.0"); }
        }
        public override string Name
        {
            get { return "Sign Protect"; }
        }
        public override string Author
        {
            get { return "Bippity"; }
        }
		public override string Description
		{
			get { return "Blocks sign editing"; }
		}

		public SignProtect(Main game) : base(game)
		{
			Order = 1;
		}

		public override void Initialize()
		{
			ServerApi.Hooks.NetGetData.Register(this, GetData);

			Commands.ChatCommands.Add(new Command("signprotect.edit", SignProtectCommand, "signprotect"));
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.NetGetData.Deregister(this, GetData);
			}
			base.Dispose(disposing);
		}

		bool enabled = true;
		public void SignProtectCommand(CommandArgs args)
		{
			enabled = !enabled;

			if (enabled)
				TSPlayer.All.SendWarningMessage("[SignProtect] Sign editing is enabled.");
			else
				TSPlayer.All.SendWarningMessage("[SignProtect] Sign editing is disabled.");
		}

		public void GetData(GetDataEventArgs e)
		{
			if (e.MsgID == PacketTypes.SignNew)
			{
				try
				{
					using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
					{
						var reader = new BinaryReader(data);
						int x = reader.ReadInt16();
						int y = reader.ReadInt16();
						reader.Close();

						int id = Terraria.Sign.ReadSign(x, y);
						TSPlayer player = TShock.Players[e.Msg.whoAmI];

						if (!player.Group.HasPermission("signprotect.edit"))
						{
							player.SendErrorMessage("[SignProtect] You do not have permission to edit this sign.");
							e.Handled = true;
							return;
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}
		}
	}
}