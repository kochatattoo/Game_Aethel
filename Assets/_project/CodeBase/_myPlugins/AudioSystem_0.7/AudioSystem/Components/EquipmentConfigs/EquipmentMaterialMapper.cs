namespace AudioSystem.Components.EquipmentConfigs
{
    public static class EquipmentMaterialMapper
    {
        public static BootsType ToBootsType(EquipmentMaterialType material)
        {
            return material switch
            {
                EquipmentMaterialType.None => BootsType.Barefoot, // или BootsType.None — зависит от логики
                EquipmentMaterialType.Leather => BootsType.Leather,
                EquipmentMaterialType.Mail => BootsType.Mail,
                EquipmentMaterialType.Plate => BootsType.Plate,
                _ => BootsType.None
            };
        }

        public static EquipmentMaterialType ToEquipmentMaterialType(BootsType boots)
        {
            return boots switch
            {
                BootsType.Barefoot => EquipmentMaterialType.None,
                BootsType.Leather => EquipmentMaterialType.Leather,
                BootsType.Mail => EquipmentMaterialType.Mail,
                BootsType.Plate => EquipmentMaterialType.Plate,
                _ => EquipmentMaterialType.None
            };
        }
    }
}
