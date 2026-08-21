using Content.Shared.CartridgeLoader;
using Content.Shared.Interaction;
using Content.Shared.Cargo.Components;
using Content.Shared.Timing;

namespace Content.Server.CartridgeLoader.Cartridges;

public sealed class AppraiseAllCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoaderSystem = default!;
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!; 
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AppraiseAllCartridgeComponent, CartridgeAddedEvent>(OnCartridgeAdded);
        SubscribeLocalEvent<AppraiseAllCartridgeComponent, CartridgeRemovedEvent>(OnCartridgeRemoved);
    }

    /// <summary>
    /// adds the components for appraising equipment upon the cartridge being installed
    /// pricegun needs usedelay in order to function, as much as it would be nice to not need it
    /// </summary>
    private void OnCartridgeAdded(Entity<AppraiseAllCartridgeComponent> ent, ref CartridgeAddedEvent args)
    {
        EnsureComp<PriceGunComponent>(args.Loader);
        EnsureComp<UseDelayComponent>(args.Loader);
    }

    /// <summary>
    /// removes the components for appraising equipment when the program is removed. does not remove it if the cartridge is removed
    /// </summary>
    private void OnCartridgeRemoved(Entity<AppraiseAllCartridgeComponent> ent, ref CartridgeRemovedEvent args)
    {
        if (!_cartridgeLoaderSystem.HasProgram<AppraiseAllCartridgeComponent>(args.Loader))
        {
            RemComp<PriceGunComponent>(args.Loader);
            RemComp<UseDelayComponent>(args.Loader);
        }
    }
}