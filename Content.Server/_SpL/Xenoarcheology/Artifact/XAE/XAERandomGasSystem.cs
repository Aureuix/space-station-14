using Content.Server._SpL.Xenoarcheology.Artifact.XAE.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Xenoarchaeology.Artifact;
using Content.Shared.Xenoarchaeology.Artifact.XAE;
using Robust.Server.GameObjects;
using Robust.Shared.Collections;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Server._SpL.Xenoarcheology.Artifact.XAE;

public sealed class XAERandomGasSystem : BaseXAESystem<XAERandomGasComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly MapSystem _map = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XAERandomGasComponent, MapInitEvent>(OnInit);
        Log.Debug("XAERandomGasSystem initialized.");
    }
    
    private void OnInit(EntityUid uid, XAERandomGasComponent component, MapInitEvent _)
    {
        Log.Debug("OnInit");
        if (component.PossibleGasses == null || component.PossibleGasses.Count == 0)
            return;

        if (true || component.SelectedGasses == null)
        {
            var GasList = new Dictionary<Gas, float>();
            var gasGas = _random.Pick(component.PossibleGasses);
            Log.Debug($"GasGas: {gasGas}");
            GasList.Add(gasGas.Key, gasGas.Value);
            

            component.SelectedGasses = GasList;
        }
    }

    protected override void OnActivated(Entity<XAERandomGasComponent> ent, ref XenoArtifactNodeActivatedEvent args)
    {
        Log.Debug("OnActivated");
        var component = ent.Comp;
        var grid = _transform.GetGrid(args.Coordinates);
        var map = _transform.GetMap(args.Coordinates);
        if (map == null || !TryComp<MapGridComponent>(grid, out var gridComp) || component.SelectedGasses == null)
            return;

        var tile = _map.LocalToTile(grid.Value, gridComp, args.Coordinates);

        var mixtures = new ValueList<GasMixture>();
        if (_atmosphere.GetTileMixture(grid.Value, map.Value, tile, excite: true) is { } localMixture)
            mixtures.Add(localMixture);

        if (_atmosphere.GetAdjacentTileMixtures(grid.Value, tile, excite: true) is var adjacentTileMixtures)
        {
            while (adjacentTileMixtures.MoveNext(out var adjacentMixture))
            {
                mixtures.Add(adjacentMixture);
            }
        }

        foreach (var (gas, moles) in component.SelectedGasses)
        {
            var molesPerMixture = moles / mixtures.Count;

            foreach (var mixture in mixtures)
            {
                mixture.AdjustMoles(gas, molesPerMixture);
            }
        }
        
        
    }
}