"use client";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faTruck, faMapMarkerAlt } from "@fortawesome/free-solid-svg-icons";

import {
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import { ShippingType, SHIPPING_TYPE_LABELS } from "@/types/auction";

import { StepProps } from "./types";

export function ShippingStep({ form }: StepProps) {
  const shippingType = form.watch("shippingType");
  const localPickupAvailable = form.watch("localPickupAvailable");

  return (
    <div className="space-y-8">
      <div className="space-y-6">
        <div className="flex items-center gap-2 text-lg font-semibold text-slate-900 dark:text-white">
          <FontAwesomeIcon icon={faTruck} className="h-5 w-5 text-orange-500" />
          Shipping Options
        </div>

        <FormField
          control={form.control}
          name="shippingType"
          render={({ field }) => (
            <FormItem className="space-y-3">
              <FormLabel>Shipping Type</FormLabel>
              <FormControl>
                <RadioGroup
                  onValueChange={field.onChange}
                  value={field.value}
                  className="flex flex-col space-y-2"
                >
                  {Object.values(ShippingType).map((type) => (
                    <div key={type} className="flex items-center space-x-2">
                      <RadioGroupItem value={type} id={type} />
                      <Label htmlFor={type}>{SHIPPING_TYPE_LABELS[type]}</Label>
                    </div>
                  ))}
                </RadioGroup>
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        {shippingType === ShippingType.Flat && (
          <FormField
            control={form.control}
            name="shippingCost"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Shipping Cost</FormLabel>
                <FormControl>
                  <div className="relative">
                    <span className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500">
                      $
                    </span>
                    <Input
                      type="number"
                      min={0}
                      step={0.01}
                      className="pl-7"
                      placeholder="0.00"
                      {...field}
                      value={field.value || ""}
                      onChange={(e) =>
                        field.onChange(parseFloat(e.target.value) || 0)
                      }
                    />
                  </div>
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        )}

        <div className="grid gap-6 md:grid-cols-2">
          <FormField
            control={form.control}
            name="handlingTime"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Handling Time (days)</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    min={1}
                    max={30}
                    {...field}
                    onChange={(e) =>
                      field.onChange(parseInt(e.target.value) || 3)
                    }
                  />
                </FormControl>
                <FormDescription>
                  Time to ship after payment (1-30 days)
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="shipsFrom"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Ships From</FormLabel>
                <FormControl>
                  <Input
                    placeholder="City, State or Country"
                    {...field}
                    value={field.value || ""}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>
      </div>

      <div className="space-y-6">
        <div className="flex items-center gap-2 text-lg font-semibold text-slate-900 dark:text-white">
          <FontAwesomeIcon icon={faMapMarkerAlt} className="h-5 w-5 text-red-500" />
          Local Pickup
        </div>

        <FormField
          control={form.control}
          name="localPickupAvailable"
          render={({ field }) => (
            <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
              <div className="space-y-0.5">
                <FormLabel className="text-base">Allow Local Pickup</FormLabel>
                <FormDescription>
                  Buyers can pick up the item in person
                </FormDescription>
              </div>
              <FormControl>
                <Switch
                  checked={field.value}
                  onCheckedChange={field.onChange}
                />
              </FormControl>
            </FormItem>
          )}
        />

        {localPickupAvailable && (
          <FormField
            control={form.control}
            name="localPickupAddress"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Pickup Address</FormLabel>
                <FormControl>
                  <Input
                    placeholder="123 Main St, City, State ZIP"
                    {...field}
                    value={field.value || ""}
                  />
                </FormControl>
                <FormDescription>
                  Full address for local pickup
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        )}
      </div>
    </div>
  );
}
