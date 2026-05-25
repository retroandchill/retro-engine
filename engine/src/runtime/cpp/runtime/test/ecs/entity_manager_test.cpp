/**
 * @file entity_manager_test.cpp
 *
 * @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */
#include <gtest/gtest.h>

import std;
import retro.runtime.ecs.entity_manager;
import retro.runtime.ecs.entity;

using namespace retro;

namespace
{
    struct TestComponent1
    {
        std::uint32_t value = 0;
    };

    struct TestComponent2
    {
        std::uint32_t value = 0;
    };

    struct TestComponent3
    {
        std::uint32_t value = 0;
    };
} // namespace

TEST(EntityManagerTest, CreatedEntityIsAlive)
{
    EntityManager manager;

    const Entity entity = manager.create_entity();

    EXPECT_TRUE(manager.is_alive(entity));
}

TEST(EntityManagerTest, DestroyedEntityIsNotAlive)
{
    EntityManager manager;

    const Entity entity = manager.create_entity();

    EXPECT_TRUE(manager.destroy_entity(entity));
    EXPECT_FALSE(manager.is_alive(entity));
}

TEST(EntityManagerTest, DestroyingInvalidEntityReturnsFalse)
{
    EntityManager manager;

    const Entity entity{
        .index = 123,
        .generation = 0,
    };

    EXPECT_FALSE(manager.destroy_entity(entity));
}

TEST(EntityManagerTest, ReusedEntitySlotGetsNewGeneration)
{
    EntityManager manager;

    const Entity first = manager.create_entity();

    ASSERT_TRUE(manager.destroy_entity(first));

    const Entity second = manager.create_entity();

    EXPECT_EQ(first.index, second.index);
    EXPECT_NE(first.generation, second.generation);
    EXPECT_FALSE(manager.is_alive(first));
    EXPECT_TRUE(manager.is_alive(second));
}

TEST(EntityManagerTest, AddAndGetComponent)
{
    EntityManager manager;

    const Entity entity = manager.create_entity();

    manager.add<TestComponent1>(entity, 42U);

    auto component = manager.try_get<TestComponent1>(entity);

    ASSERT_TRUE(component.has_value());
    EXPECT_EQ(component->value, 42U);
}

TEST(EntityManagerTest, TryGetMissingComponentReturnsEmpty)
{
    EntityManager manager;

    const Entity entity = manager.create_entity();

    auto component = manager.try_get<TestComponent1>(entity);

    EXPECT_FALSE(component.has_value());
}

TEST(EntityManagerTest, RemoveComponentMakesItUnavailable)
{
    EntityManager manager;

    const Entity entity = manager.create_entity();

    manager.add<TestComponent1>(entity, 42U);

    ASSERT_TRUE(manager.try_get<TestComponent1>(entity).has_value());

    manager.remove<TestComponent1>(entity);

    EXPECT_FALSE(manager.try_get<TestComponent1>(entity).has_value());
}

TEST(EntityManagerTest, DestroyEntityRemovesItsComponents)
{
    EntityManager manager;

    const Entity entity = manager.create_entity();

    manager.add<TestComponent1>(entity, 42U);
    manager.add<TestComponent2>(entity, 99U);

    ASSERT_TRUE(manager.try_get<TestComponent1>(entity).has_value());
    ASSERT_TRUE(manager.try_get<TestComponent2>(entity).has_value());

    ASSERT_TRUE(manager.destroy_entity(entity));

    EXPECT_FALSE(manager.try_get<TestComponent1>(entity).has_value());
    EXPECT_FALSE(manager.try_get<TestComponent2>(entity).has_value());
}

TEST(EntityManagerTest, AddComponentToDeadEntityThrows)
{
    EntityManager manager;

    const Entity entity = manager.create_entity();

    ASSERT_TRUE(manager.destroy_entity(entity));

    EXPECT_THROW(manager.add<TestComponent1>(entity, 42U), std::invalid_argument);
}

TEST(EntityManagerTest, AddingSameComponentTypeReplacesExistingComponent)
{
    EntityManager manager;

    const Entity entity = manager.create_entity();

    manager.add<TestComponent1>(entity, 1U);
    manager.add<TestComponent1>(entity, 2U);

    auto component = manager.try_get<TestComponent1>(entity);

    ASSERT_TRUE(component.has_value());
    EXPECT_EQ(component->value, 2U);
}

TEST(EntityManagerTest, SingleComponentViewIteratesMatchingEntities)
{
    EntityManager manager;

    const Entity first = manager.create_entity();
    const Entity second = manager.create_entity();
    const Entity third = manager.create_entity();

    manager.add<TestComponent1>(first, 1U);
    manager.add<TestComponent2>(second, 2U);
    manager.add<TestComponent1>(third, 3U);

    std::vector<std::uint32_t> values;

    for (auto [entity, component] : manager.view<TestComponent1>())
    {
        values.push_back(component.value);
    }

    EXPECT_EQ(values.size(), 2U);
    EXPECT_NE(std::ranges::find(values, 1U), values.end());
    EXPECT_NE(std::ranges::find(values, 3U), values.end());
}

TEST(EntityManagerTest, MultiComponentViewIteratesOnlyEntitiesWithAllComponents)
{
    EntityManager manager;

    const Entity first = manager.create_entity();
    const Entity second = manager.create_entity();
    const Entity third = manager.create_entity();

    manager.add<TestComponent1>(first, 1U);
    manager.add<TestComponent2>(first, 10U);

    manager.add<TestComponent1>(second, 2U);

    manager.add<TestComponent1>(third, 3U);
    manager.add<TestComponent2>(third, 30U);

    std::vector<Entity> entities;
    std::vector<std::uint32_t> component1_values;
    std::vector<std::uint32_t> component2_values;

    for (auto [entity, component1, component2] : manager.view<TestComponent1, TestComponent2>())
    {
        entities.push_back(entity);
        component1_values.push_back(component1.value);
        component2_values.push_back(component2.value);
    }

    EXPECT_EQ(entities.size(), 2U);

    EXPECT_NE(std::ranges::find(entities, first), entities.end());
    EXPECT_EQ(std::ranges::find(entities, second), entities.end());
    EXPECT_NE(std::ranges::find(entities, third), entities.end());

    EXPECT_NE(std::ranges::find(component1_values, 1U), component1_values.end());
    EXPECT_NE(std::ranges::find(component1_values, 3U), component1_values.end());

    EXPECT_NE(std::ranges::find(component2_values, 10U), component2_values.end());
    EXPECT_NE(std::ranges::find(component2_values, 30U), component2_values.end());
}

TEST(EntityManagerTest, MultiComponentViewAllowsMutation)
{
    EntityManager manager;

    const Entity entity = manager.create_entity();

    manager.add<TestComponent1>(entity, 1U);
    manager.add<TestComponent2>(entity, 10U);

    for (auto [current, component1, component2] : manager.view<TestComponent1, TestComponent2>())
    {
        component1.value += 1U;
        component2.value += 2U;
    }

    auto component1 = manager.try_get<TestComponent1>(entity);
    auto component2 = manager.try_get<TestComponent2>(entity);

    ASSERT_TRUE(component1.has_value());
    ASSERT_TRUE(component2.has_value());

    EXPECT_EQ(component1->value, 2U);
    EXPECT_EQ(component2->value, 12U);
}

TEST(EntityManagerTest, MultiComponentViewUpdatesAfterComponentRemoval)
{
    EntityManager manager;

    const Entity first = manager.create_entity();
    const Entity second = manager.create_entity();

    manager.add<TestComponent1>(first, 1U);
    manager.add<TestComponent2>(first, 10U);

    manager.add<TestComponent1>(second, 2U);
    manager.add<TestComponent2>(second, 20U);

    manager.remove<TestComponent2>(first);

    std::vector<Entity> entities;

    for (auto [entity, component1, component2] : manager.view<TestComponent1, TestComponent2>())
    {
        entities.push_back(entity);
    }

    EXPECT_EQ(entities.size(), 1U);
    EXPECT_EQ(entities.front(), second);
}

TEST(EntityManagerTest, ThreeComponentViewIteratesOnlyEntitiesWithAllComponents)
{
    EntityManager manager;

    const Entity first = manager.create_entity();
    const Entity second = manager.create_entity();

    manager.add<TestComponent1>(first, 1U);
    manager.add<TestComponent2>(first, 2U);
    manager.add<TestComponent3>(first, 3U);

    manager.add<TestComponent1>(second, 10U);
    manager.add<TestComponent2>(second, 20U);

    std::size_t count = 0;

    for (auto [entity, component1, component2, component3] :
         manager.view<TestComponent1, TestComponent2, TestComponent3>())
    {
        EXPECT_EQ(entity, first);
        EXPECT_EQ(component1.value, 1U);
        EXPECT_EQ(component2.value, 2U);
        EXPECT_EQ(component3.value, 3U);
        ++count;
    }

    EXPECT_EQ(count, 1U);
}

TEST(EntityManagerTest, ViewOfMissingComponentPoolIsEmpty)
{
    EntityManager manager;

    const Entity entity = manager.create_entity();

    manager.add<TestComponent1>(entity, 1U);

    std::size_t count = 0;

    for (auto [current, component] : manager.view<TestComponent2>())
    {
        ++count;
    }

    EXPECT_EQ(count, 0U);
}

TEST(EntityManagerTest, MultiComponentViewWithMissingFirstPoolIsEmpty)
{
    EntityManager manager;

    const Entity entity = manager.create_entity();

    manager.add<TestComponent2>(entity, 2U);

    std::size_t count = 0;

    for (auto [current, component1, component2] : manager.view<TestComponent1, TestComponent2>())
    {
        ++count;
    }

    EXPECT_EQ(count, 0U);
}
